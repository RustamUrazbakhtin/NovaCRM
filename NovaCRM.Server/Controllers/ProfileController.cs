using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaCRM.Server.Auth;
using NovaCRM.Server.Contracts;
using NovaCRM.Server.Domain;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private static readonly string[] AllowedAvatarExtensions = new[] { ".jpg", ".jpeg", ".png" };
    private const long MaxAvatarSizeBytes = 2 * 1024 * 1024; // 2 MB

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        ILogger<ProfileController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var (user, staff) = await FindCurrentUserAsync(cancellationToken);
        if (user is null || staff is null)
        {
            return NotFound();
        }

        return Ok(ToDto(staff, user));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateAsync(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (user, staff) = await FindCurrentUserAsync(cancellationToken);
        if (user is null || staff is null)
        {
            return NotFound();
        }

        UpdateStaff(staff, request);
        staff.UpdatedAt = DateTime.UtcNow;

        var email = request.Email.Trim();

        var emailResult = await _userManager.SetEmailAsync(user, email);
        if (!emailResult.Succeeded)
        {
            foreach (var error in emailResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var userNameResult = await _userManager.SetUserNameAsync(user, email);
        if (!userNameResult.Succeeded)
        {
            foreach (var error in userNameResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update profile for user {UserId}", user.Id);
            return Problem("Failed to save profile changes. Please try again.", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(ToDto(staff, user));
    }

    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxAvatarSizeBytes + 1024)]
    [RequestSizeLimit(MaxAvatarSizeBytes + 1024)]
    public async Task<ActionResult<UserProfileDto>> UploadAvatarAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Please choose an image file." });
        }

        if (file.Length > MaxAvatarSizeBytes)
        {
            return BadRequest(new { message = "Avatar must be 2 MB or smaller." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only JPG and PNG images are supported." });
        }

        var (user, staff) = await FindCurrentUserAsync(cancellationToken);
        if (user is null || staff is null)
        {
            return NotFound();
        }

        var uploadsFolder = EnsureAvatarUploadFolder();
        var newFileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var fullPath = Path.Combine(uploadsFolder, newFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        RemoveAvatarFileIfExists(staff.AvatarUrl);

        var relativePath = $"/uploads/avatars/{newFileName}";
        staff.AvatarUrl = relativePath;
        staff.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update avatar for user {UserId}", user.Id);
            return Problem("Failed to save profile changes. Please try again.", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(ToDto(staff, user));
    }

    [HttpDelete("me/avatar")]
    public async Task<ActionResult<UserProfileDto>> DeleteAvatarAsync(CancellationToken cancellationToken)
    {
        var (user, staff) = await FindCurrentUserAsync(cancellationToken);
        if (user is null || staff is null)
        {
            return NotFound();
        }

        RemoveAvatarFileIfExists(staff.AvatarUrl);
        staff.AvatarUrl = null;
        staff.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to remove avatar for user {UserId}", user.Id);
            return Problem("Failed to save profile changes. Please try again.", statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(ToDto(staff, user));
    }

    private async Task<(ApplicationUser? User, Staff? Staff)> FindCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return (null, null);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return (null, null);
        }

        var staff = await _dbContext.StaffMembers.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (staff is null)
        {
            return (user, null);
        }

        return (user, staff);
    }

    private static void UpdateStaff(Staff staff, UpdateUserProfileRequest request)
    {
        staff.FirstName = request.FirstName.Trim();
        staff.LastName = request.LastName.Trim();
        staff.RoleTitle = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role.Trim();
        staff.Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim();
        staff.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        staff.Timezone = string.IsNullOrWhiteSpace(request.Timezone) ? null : request.Timezone.Trim();
        staff.Locale = string.IsNullOrWhiteSpace(request.Locale) ? null : request.Locale.Trim();
        staff.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        staff.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
    }

    private UserProfileDto ToDto(Staff staff, ApplicationUser user)
    {
        var updatedAt = staff.UpdatedAt == default
            ? DateTime.UtcNow
            : DateTime.SpecifyKind(staff.UpdatedAt, DateTimeKind.Utc);

        return new UserProfileDto(
            staff.Id,
            staff.FirstName,
            staff.LastName,
            user.Email ?? string.Empty,
            staff.Phone,
            staff.RoleTitle,
            staff.Company,
            staff.Timezone,
            staff.Locale,
            staff.Address,
            staff.Notes,
            staff.AvatarUrl,
            updatedAt.ToString("O", CultureInfo.InvariantCulture)
        );
    }

    private string EnsureAvatarUploadFolder()
    {
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }
        }

        var uploadsFolder = Path.Combine(webRoot, "uploads", "avatars");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        return uploadsFolder;
    }

    private void RemoveAvatarFileIfExists(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            return;
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var relativePath = avatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webRoot, relativePath);

        try
        {
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to delete old avatar file at {FilePath}", fullPath);
        }
    }
}
