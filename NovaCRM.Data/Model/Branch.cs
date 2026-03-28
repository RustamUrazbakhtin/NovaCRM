namespace NovaCRM.Data.Model;

public partial class Branch
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
