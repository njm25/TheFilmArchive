using Api.Requests;
using Api.Responses;
using Api.Services;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;
    private readonly JwtService _jwtService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserController> _logger;
    private readonly EmailQuotaService _emailQuota;
    private readonly IMemoryCache _cache;

    public UserController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext db,
        JwtService jwtService,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<UserController> logger,
        EmailQuotaService emailQuota,
        IMemoryCache cache
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _jwtService = jwtService;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
        _emailQuota = emailQuota;
        _cache = cache;
    }

    [HttpPost("requestAccount")]
    [EnableRateLimiting(RateLimitPolicies.OutboundEmail)]
    public async Task<IActionResult> RequestAccount([FromBody] RequestAccountReq req)
    {
        try
        {
            var addr = new MailAddress(req.Email);
            if (!string.Equals(addr.Address, req.Email, StringComparison.OrdinalIgnoreCase))
                throw new Exception();
        }
        catch
        {
            return BadRequest(new { message = "Invalid email format." });
        }

        // Every path below this point answers with exactly this, so the response
        // can't be used to test whether an address is registered or has a link
        // in flight. What differs is only what lands in the mailbox, which the
        // requester can't see unless they own it.
        IActionResult accepted = Ok(new { message = "Account request sent." });

        string clientBaseUrl = (_configuration["Site:BaseUrl"] ?? "https://thefilmarchive.org").TrimEnd('/');
        int cooldownMinutes = _configuration.GetValue<int?>("Email:PerAddressCooldownMinutes") ?? 5;

        // Per-IP limiting doesn't help the person on the receiving end: someone
        // rotating IPs could still bury one address in mail. This cooldown is
        // keyed on the address, so a mailbox can only be written to once per
        // window regardless of where the requests come from. Held in the cache
        // rather than the database because it has to cover addresses that never
        // get an AccountRequest row - the already-registered ones.
        string cooldownKey = $"registration-email:{req.Email.ToLowerInvariant()}";

        if (_cache.TryGetValue(cooldownKey, out _))
            return accepted;

        // Claimed before anything is written or sent, so a refusal leaves no
        // half-finished request behind and doesn't rotate a token we aren't
        // going to deliver.
        if (!_emailQuota.TryReserve())
        {
            _logger.LogWarning("Registration email refused: daily quota exhausted.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Sign-ups are temporarily unavailable. Please try again later." }
            );
        }

        // An address that already has an account gets told so by email rather
        // than by the response, so the owner still finds out someone tried.
        var existingUserByEmail = await _userManager.FindByEmailAsync(req.Email);

        if (existingUserByEmail != null)
        {
            if (!await TrySendAsync(
                    req.Email,
                    "You already have a Film Archive account",
                    EmailTemplates.AccountAlreadyExists(clientBaseUrl)))
            {
                return MailFailure();
            }

            StartCooldown(cooldownKey, cooldownMinutes);

            return accepted;
        }

        AccountRequest? existingRequest = await _db.AccountRequests
            .FirstOrDefaultAsync(x => x.Email == req.Email);

        string token = GenerateToken();

        if (existingRequest != null)
        {
            existingRequest.Token = token;
            existingRequest.CreatedUtc = DateTime.UtcNow;
        }
        else
        {
            AccountRequest accountRequest = new AccountRequest
            {
                Email = req.Email,
                Token = token,
                CreatedUtc = DateTime.UtcNow
            };

            _db.AccountRequests.Add(accountRequest);
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "Could not create account request." });
        }

        string registrationLink = $"{clientBaseUrl}/register/{token}";

        if (!await TrySendAsync(
                req.Email,
                "Finish creating your Film Archive account",
                EmailTemplates.RegistrationInvite(clientBaseUrl, registrationLink)))
        {
            return MailFailure();
        }

        StartCooldown(cooldownKey, cooldownMinutes);

        return accepted;
    }

    // Both send paths report a failure identically, so an outage looks the same
    // for a registered address as for an unknown one.
    private IActionResult MailFailure() => StatusCode(
        StatusCodes.Status502BadGateway,
        new { message = "Could not send the email. Please try again." }
    );

    private async Task<bool> TrySendAsync(string toAddress, string subject, string htmlBody)
    {
        try
        {
            await _emailSender.SendAsync(toAddress, subject, htmlBody);
            return true;
        }
        catch (Exception ex)
        {
            // Logged rather than surfaced - the caller reports a generic failure.
            _logger.LogError(ex, "Could not send a registration email.");
            return false;
        }
    }

    // Only started once mail is actually away, so a transient outage doesn't
    // lock someone out of retrying for the whole window.
    private void StartCooldown(string key, int minutes) =>
        _cache.Set(key, true, TimeSpan.FromMinutes(minutes));

    [HttpPost("forgotPassword")]
    [EnableRateLimiting(RateLimitPolicies.OutboundEmail)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReq req)
    {
        // Identical answer whether or not the address has an account, otherwise
        // this endpoint becomes a way to test which emails are registered.
        IActionResult accepted = Ok(new { message = "Password reset email sent." });

        string clientBaseUrl = (_configuration["Site:BaseUrl"] ?? "https://thefilmarchive.org").TrimEnd('/');
        int cooldownMinutes = _configuration.GetValue<int?>("Email:PerAddressCooldownMinutes") ?? 5;

        string cooldownKey = $"password-reset:{req.Email.ToLowerInvariant()}";

        if (_cache.TryGetValue(cooldownKey, out _))
            return accepted;

        var user = await _userManager.FindByEmailAsync(req.Email);

        // Looked up before the quota is touched on purpose: charging unknown
        // addresses would let someone drain the daily allowance with made-up
        // emails and lock real users out of resetting their passwords.
        if (user == null)
            return accepted;

        // Answered as success rather than 503 - a 503 here would only ever be
        // reachable for a real account, which is exactly the tell this endpoint
        // is trying not to give. Operators find out through the log.
        if (!_emailQuota.TryReserve())
        {
            _logger.LogWarning("Password reset email refused: daily quota exhausted.");

            return accepted;
        }

        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        string resetLink = $"{clientBaseUrl}/resetPassword/{PackResetToken(user.Id, resetToken)}";

        if (!await TrySendAsync(
                req.Email,
                "Reset your Film Archive password",
                EmailTemplates.PasswordReset(clientBaseUrl, resetLink)))
        {
            return MailFailure();
        }

        StartCooldown(cooldownKey, cooldownMinutes);

        return accepted;
    }

    [HttpPost("resetPassword")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReq req)
    {
        IActionResult invalid = BadRequest(new
        {
            message = "This reset link is invalid or has expired. Request a new one."
        });

        if (!TryUnpackResetToken(req.Token, out string userId, out string resetToken))
            return invalid;

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return invalid;

        // Succeeding here rotates the user's security stamp, which is what makes
        // the link single-use - a second attempt with the same token fails.
        IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, req.Password);

        if (result.Succeeded)
            return Ok(new { message = "Password updated." });

        // A spent or expired token and a too-weak password both land here. Only
        // the password rules are worth showing back; a bad token gets the same
        // wording as a link that wouldn't even parse.
        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken)))
            return invalid;

        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    // The reset token is Identity's own - signed, self-expiring, and invalidated
    // once the password changes - but ResetPasswordAsync needs the user too. The
    // link carries both, packed into one URL-safe blob, rather than putting the
    // email address in a query string where it would end up in logs and history.
    private static string PackResetToken(string userId, string token) =>
        WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes($"{userId}|{token}"));

    private static bool TryUnpackResetToken(string packed, out string userId, out string token)
    {
        userId = string.Empty;
        token = string.Empty;

        try
        {
            string payload = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(packed));
            int split = payload.IndexOf('|');

            if (split <= 0 || split == payload.Length - 1)
                return false;

            userId = payload[..split];
            token = payload[(split + 1)..];

            return true;
        }
        catch
        {
            // Anything unparseable is just an invalid link.
            return false;
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq req)
    {
        AccountRequest? accountRequest = await _db.AccountRequests
            .FirstOrDefaultAsync(o => o.Token == req.Token);

        if (accountRequest == null)
            return BadRequest(new { message = "Invalid." });

        if (accountRequest.CreatedUtc < DateTime.UtcNow.AddHours(-24))
            return BadRequest(new { message = "Request expired." });

        var existingUserByEmail = await _userManager.FindByEmailAsync(accountRequest.Email);
        if (existingUserByEmail != null)
            return BadRequest(new { message = "Email is already in use." });

        var existingUserByName = await _userManager.FindByNameAsync(req.UserName);
        if (existingUserByName != null)
            return BadRequest(new { message = "Username is already in use." });

        var user = new ApplicationUser
        {
            Email = accountRequest.Email,
            UserName = req.UserName
        };

        var result = await _userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });
        }

        _db.AccountRequests.Remove(accountRequest);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IActionResult> Login([FromBody] LoginReq req)
    {
        var user = await _userManager.FindByNameAsync(req.UserNameOrEmail);

        if (user == null)
            user = await _userManager.FindByEmailAsync(req.UserNameOrEmail);

        if (user == null)
            return Unauthorized(new { message = "Invalid credentials." });

        var validPassword = await _userManager.CheckPasswordAsync(user, req.Password);

        if (!validPassword)
            return Unauthorized(new { message = "Invalid credentials." });

        string token = await _jwtService.GenerateTokenAsync(user);

        return Ok(new
        {
            message = "Login successful.",
            token
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var roleString = User.FindFirstValue(ClaimTypes.Role);

        var role = Enum.TryParse<RoleEnum>(roleString, out var parsedRole)
            ? parsedRole
            : RoleEnum.User;

        MeRes res = new MeRes()
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            UserName = User.Identity?.Name ?? "",
            Role = role
        };

        return Ok(res);
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersReq req)
    {
        IQueryable <ApplicationUser> query = _db.Users.AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(req.SearchText))
        {
            query = query.Where(f => f.UserName != null && f.UserName.Contains(req.SearchText));
        }

        // Ordering
        query = (req.OrderBy, req.OrderingType) switch
        {

            (OrderUserByEnum.Id, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.Id),

            (OrderUserByEnum.Id, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.Id),

            (OrderUserByEnum.Email, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.Email),

            (OrderUserByEnum.Email, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.Email),

            (OrderUserByEnum.UserName, OrderingTypeEnum.Ascending) =>
                query.OrderBy(f => f.UserName),

            (OrderUserByEnum.UserName, OrderingTypeEnum.Descending) =>
                query.OrderByDescending(f => f.UserName),

            _ => query.OrderBy(f => f.Id)
        };

        // Paging
        int skip = (req.PageNumber - 1) * req.PageSize;

        List<GetUsersResItem> users = await query
            .Skip(skip)
            .Take(req.PageSize)
            .Select(f => new GetUsersResItem
            {
                UserName = f.UserName ?? "Username not found.",
                Id = f.Id,
                Email = f.Email ?? "Email not found.",
                Role = f.Role
            })
            .ToListAsync();

        var res = new GetUsersRes
        {
            Users = users
        };

        return Ok(res);
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpPost("setRole/{userId}")]
    public async Task<IActionResult> SetRole(string userId, [FromBody] RoleEnum role)
    {
        ApplicationUser? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
            return NotFound();

        user.Role = role;

        await _db.SaveChangesAsync();

        return Ok();
    }

    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}