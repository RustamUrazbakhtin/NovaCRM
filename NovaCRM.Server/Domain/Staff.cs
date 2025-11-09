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

    public Organization? Organization { get; set; }
    public Branch? Branch { get; set; }
}
