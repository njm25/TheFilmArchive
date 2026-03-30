using Api.Requests;
using Api.Responses;
using Api.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;
    private readonly JwtService _jwtService;

    public UserController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext db,
        JwtService jwtService
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _jwtService = jwtService;
    }

    [HttpPost("requestAccount")]
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

        var existingUserByEmail = await _userManager.FindByEmailAsync(req.Email);
        if (existingUserByEmail != null)
            return BadRequest(new { message = "Email is already in use." });

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

        // to-do send email with token

        return Ok(new
        {
            message = "Account request sent."
        });
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

    [Authorize(Roles = "SysAdmin")]
    [HttpGet("accountRequests")]
    public async Task<IActionResult> GetAccountRequests()
    {
        List<GetAccountRequestsResItem> accountRequests = await _db.AccountRequests
            .OrderByDescending(o => o.CreatedUtc)
            .Select(o => new GetAccountRequestsResItem
            {
                Email = o.Email,
                Token = o.Token,
            })       
            .ToListAsync();

        GetAccountRequestsRes res = new GetAccountRequestsRes
        {
            AccountRequests = accountRequests
        };

        return Ok(res);
    }

    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}