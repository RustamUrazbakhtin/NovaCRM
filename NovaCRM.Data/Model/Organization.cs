namespace NovaCRM.Data.Model;

public partial class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Timezone { get; set; } = null!;
    public string Currency { get; set; } = "USD";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string PlanType { get; set; } = "free";
    public string SubscriptionStatus { get; set; } = "active";
    public string Settings { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    public virtual ICollection<ClientTag> ClientTags { get; set; } = new List<ClientTag>();
    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public virtual ICollection<StaffRole> StaffRoles { get; set; } = new List<StaffRole>();
    public virtual ICollection<StaffSpecialization> StaffSpecializations { get; set; } = new List<StaffSpecialization>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
