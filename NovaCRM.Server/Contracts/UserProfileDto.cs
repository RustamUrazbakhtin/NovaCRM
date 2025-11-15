using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCRM.Server.Contracts;

public record UserProfileDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? RoleId,
    string? RoleName,
    string? CompanyId,
    string? CompanyName,
    string? Timezone,
    string? Locale,
    string? Address,
    string? Notes,
    string? AvatarUrl,
    string UpdatedAt
);

public class UpdateUserProfileRequest
{
    [Required]
    [StringLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(64)]
    public string? Phone { get; set; }

    [StringLength(64)]
    public string? RoleId { get; set; }

    [StringLength(64)]
    public string? Timezone { get; set; }

    [StringLength(32)]
    public string? Locale { get; set; }

    [StringLength(512)]
    public string? Address { get; set; }

    [StringLength(1024)]
    public string? Notes { get; set; }
}

public record ProfileRoleOptionDto(string Id, string Name);
