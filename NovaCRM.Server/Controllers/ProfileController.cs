using System;
using System.Collections.Generic;
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
using NovaCRM.Data;
using NovaCRM.Data.Auth;
using NovaCRM.Data.Model;
using NovaCRM.Server.Contracts;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private static readonly string[] AllowedAvatarExtensions = new[] { ".jpg", ".jpeg", ".png" };
    private const long MaxAvatarSizeBytes = 2 * 1024 * 1024; // 2 MB

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<AspNetUser> _userManager;
    private readonly RoleManager<AspNetRole> _roleManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        ApplicationDbContext dbContext,
        UserManager<AspNetUser> userManager,
        RoleManager<AspNetRole> roleManager,
        IWebHostEnvironment environment,
        ILogger<ProfileController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        try
        {
            var (user, staff) = await FindCurrentUserAsync(cancellationToken);
            if (user is null || staff is null)
            {
                return NotFound();
            }

            var (roleId, roleName) = await GetPrimaryRoleAsync(user.Id, cancellationToken);
            var dto = await ToDtoAsync(staff, user, roleId, roleName, cancellationToken);

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load profile for the current user {UserId}", _userManager.GetUserId(User));
            return Problem("Unable to load profile. Please try again later.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<ProfileRoleOptionDto>>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new ProfileRoleOptionDto(role.Id, role.Name ?? string.Empty))
            .ToListAsync(cancellationToken);

        return Ok(roles);
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

        AspNetRole? selectedRole = null;
        var requestedRoleId = request.RoleId?.Trim();
        if (!string.IsNullOrWhiteSpace(requestedRoleId))
        {
            selectedRole = await _dbContext.AspNetRoles.FirstOrDefaultAsync(
                role => role.Id == requestedRoleId,
                cancellationToken);

            if (selectedRole is null || string.IsNullOrWhiteSpace(selectedRole.Name))
            {
                ModelState.AddModelError(nameof(request.RoleId), "Selected role is not available.");
                return ValidationProblem(ModelState);
            }
        }

        UpdateStaff(staff, request, selectedRole?.Name);
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

        var existingRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = selectedRole is null
            ? existingRoles.ToArray()
            : existingRoles
                .Where(name => !string.Equals(name, selectedRole.Name, StringComparison.OrdinalIgnoreCase))
                .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem(ModelState);
            }
        }

        if (selectedRole is not null
            && !existingRoles.Any(name => string.Equals(name, selectedRole.Name, StringComparison.OrdinalIgnoreCase)))
        {
            var addResult = await _userManager.AddToRoleAsync(user, selectedRole.Name);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem(ModelState);
            }
        }

        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        if (!string.Equals(user.PhoneNumber, phone, StringComparison.Ordinal))
        {
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, phone);
            if (!phoneResult.Succeeded)
            {
                foreach (var error in phoneResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem(ModelState);
            }
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

        var (roleId, roleName) = await GetPrimaryRoleAsync(user.Id, cancellationToken);
        var dto = await ToDtoAsync(staff, user, roleId, roleName, cancellationToken);

        return Ok(dto);
    }

    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxAvatarSizeBytes + 1024)]
    [RequestSizeLimit(MaxAvatarSizeBytes + 1024)]
    public async Task<ActionResult<UserProfileDto>> UploadAvatarAsync(
        [FromForm] UploadAvatarForm form,
        CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
        {
            return BadRequest(new { message = "Please choose an image file." });
        }

        if (form.File.Length > MaxAvatarSizeBytes)
        {
            return BadRequest(new { message = "Avatar must be 2 MB or smaller." });
        }

        var extension = Path.GetExtension(form.File.FileName).ToLowerInvariant();
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
            await form.File.CopyToAsync(stream, cancellationToken);
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

        var (roleId, roleName) = await GetPrimaryRoleAsync(user.Id, cancellationToken);
        var dto = await ToDtoAsync(staff, user, roleId, roleName, cancellationToken);

        return Ok(dto);
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

        var (roleId, roleName) = await GetPrimaryRoleAsync(user.Id, cancellationToken);
        var dto = await ToDtoAsync(staff, user, roleId, roleName, cancellationToken);

        return Ok(dto);
    }

    private async Task<(AspNetUser? User, Staff? Staff)> FindCurrentUserAsync(CancellationToken cancellationToken)
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

        var staff = await _dbContext.Staff
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (staff is null)
        {
            return (user, null);
        }

        return (user, staff);
    }

    private async Task<(string? RoleId, string? RoleName)> GetPrimaryRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userRoles = await _dbContext.AspNetUserRoles
            .Where(ur => ur.UserId == userId)
            .Join(
                _dbContext.AspNetRoles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.RoleId, role.Name });
            .ToListAsync(cancellationToken);

        if (userRoles.Count == 0)
        {
            return (null, null);
        }

        var primaryRole = userRoles.First();

        return (primaryRole.RoleId, primaryRole.Name);
    }

    private static void UpdateStaff(Staff staff, UpdateUserProfileRequest request, string? resolvedRoleName)
    {
        staff.FirstName = request.FirstName.Trim();
        staff.LastName = request.LastName.Trim();
        staff.RoleTitle = resolvedRoleName;
        staff.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        //staff.Timezone = string.IsNullOrWhiteSpace(request.Timezone) ? null : request.Timezone.Trim();
        //staff.Locale = string.IsNullOrWhiteSpace(request.Locale) ? null : request.Locale.Trim();
        //staff.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        //staff.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
    }

    private async Task<UserProfileDto> ToDtoAsync(
        Staff staff,
        AspNetUser user,
        string? roleId,
        string? roleName,
        CancellationToken cancellationToken)
    {
        var organization = staff.Organization;
        if (organization is null && staff.OrganizationId != Guid.Empty)
        {
            organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == staff.OrganizationId, cancellationToken);
        }

        var updatedAt = staff.UpdatedAt == default
            ? DateTime.UtcNow
            : DateTime.SpecifyKind(staff.UpdatedAt, DateTimeKind.Utc);

        var companyIdValue = NormalizeString(staff.OrganizationId != Guid.Empty
            ? staff.OrganizationId.ToString()
            : organization?.Id.ToString());
        var companyName = NormalizeString(organization?.Name ?? staff.Company);
        var resolvedRoleName = NormalizeString(roleName ?? staff.RoleTitle);
        var resolvedRoleId = NormalizeString(roleId);
        var phone = NormalizeString(staff.Phone ?? user.PhoneNumber);
        var timezone = NormalizeString(staff.Timezone);
        var locale = NormalizeString(staff.Locale);
        var address = NormalizeString(staff.Address);
        var notes = NormalizeString(staff.Notes);
        var avatarUrl = NormalizeString(staff.AvatarUrl);

        return new UserProfileDto(
            staff.Id.ToString(),
            NormalizeString(staff.FirstName),
            NormalizeString(staff.LastName),
            NormalizeString(user.Email),
            phone,
            resolvedRoleId,
            resolvedRoleName,
            companyIdValue,
            companyName,
            timezone,
            locale,
            address,
            notes,
            avatarUrl,
            updatedAt.ToString("O", CultureInfo.InvariantCulture)
        );
    }

    private static string NormalizeString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
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
