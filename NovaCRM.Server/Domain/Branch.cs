using System;
using System.Collections.Generic;

namespace NovaCRM.Server.Domain;

public class Branch
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Timezone { get; set; }
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }

    public Organization? Organization { get; set; }
    public ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
}
