using System;

namespace NovaCRM.Server.Domain;

public class Staff
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public string? UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? RoleTitle { get; set; }
    public bool IsActive { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Timezone { get; set; }
    public string? Locale { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Organization? Organization { get; set; }
    public Branch? Branch { get; set; }
}
