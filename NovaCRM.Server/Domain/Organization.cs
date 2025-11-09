using System;
using System.Collections.Generic;

namespace NovaCRM.Server.Domain;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Industry { get; set; }
    public string Country { get; set; } = null!;
    public string Timezone { get; set; } = null!;
    public string Currency { get; set; } = "USD";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? BusinessId { get; set; }
    public string PlanType { get; set; } = "free";
    public string SubscriptionStatus { get; set; } = "active";

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
}
