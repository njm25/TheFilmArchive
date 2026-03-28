using Api.Requests;
using Api.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
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

    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}