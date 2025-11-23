using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;
using NovaCRM.Server.Contracts;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<AspNetUser> _passwordHasher;
    private readonly IConfiguration _cfg;
    private readonly IRegistrationService _registrationService;

    public AuthController(
        ApplicationDbContext dbContext,
        IPasswordHasher<AspNetUser> passwordHasher,
        IConfiguration cfg,
        IRegistrationService registrationService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _cfg = cfg;
        _registrationService = registrationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (success, response, errors) = await _registrationService.RegisterOrganizationAsync(request, cancellationToken);

        if (!success)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return ValidationProblem(ModelState);
        }

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();

        var user = await _dbContext.AspNetUsers
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Unauthorized();
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var token = CreateJwt(user);
        return Ok(new { token });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == null) return Unauthorized();
        var user = await _dbContext.AspNetUsers.FindAsync(id);
        return Ok(new { user!.Email });
    }

    private string CreateJwt(AspNetUser user)
    {
        var jwt = _cfg.GetSection("Jwt");
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        foreach (var role in user.Roles)
        {
            if (!string.IsNullOrWhiteSpace(role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims,
            expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginDto(string Email, string Password);
}
