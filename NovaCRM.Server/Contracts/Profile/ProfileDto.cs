using System;
using System.ComponentModel.DataAnnotations;

namespace NovaCRM.Server.Contracts.Profile;

public class ProfileDto
{
    public Guid UserId { get; set; }

    public Guid StaffId { get; set; }

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Notes { get; set; }
}

public class UpdateProfileDto
{
    [Required]
    [StringLength(128)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(128)]
    public string LastName { get; set; } = null!;

    [StringLength(64)]
    public string? Phone { get; set; }

    [StringLength(512)]
    public string? Address { get; set; }

    [StringLength(1024)]
    public string? Notes { get; set; }
}
