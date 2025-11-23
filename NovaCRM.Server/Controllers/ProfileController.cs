using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Server.Contracts.Profile;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public ProfileController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.AspNetUsers
            .FirstOrDefaultAsync(u => u.Id == userIdClaim, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var staff = await _dbContext.Staff
            .FirstOrDefaultAsync(s => s.UserId == userIdClaim, cancellationToken);

        if (staff is null)
        {
            return NotFound("Staff record not found for current user.");
        }

        var dto = new ProfileDto
        {
            UserId = userId,
            StaffId = staff.Id,
            Email = user.Email ?? string.Empty,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Phone = staff.Phone,
            Address = staff.Address,
            Notes = staff.Notes
        };

        return Ok(dto);
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(
        [FromBody] UpdateProfileDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.AspNetUsers
            .FirstOrDefaultAsync(u => u.Id == userIdClaim, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var staff = await _dbContext.Staff
            .FirstOrDefaultAsync(s => s.UserId == userIdClaim, cancellationToken);
        if (staff is null)
        {
            return NotFound("Staff record not found.");
        }

        staff.FirstName = dto.FirstName.Trim();
        staff.LastName = dto.LastName.Trim();
        staff.Phone = dto.Phone;
        staff.Address = dto.Address;
        staff.Notes = dto.Notes;
        staff.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new ProfileDto
        {
            UserId = userId,
            StaffId = staff.Id,
            Email = user.Email ?? string.Empty,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Phone = staff.Phone,
            Address = staff.Address,
            Notes = staff.Notes
        };

        return Ok(result);
    }
}
