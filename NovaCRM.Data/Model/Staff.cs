namespace NovaCRM.Data.Model;

public partial class Staff
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public string? UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? RoleTitle { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime? Birthday { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual AspNetUser? User { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<StaffRoleLink> StaffRoleLinks { get; set; } = new List<StaffRoleLink>();
    public virtual ICollection<StaffSpecializationLink> StaffSpecializationLinks { get; set; } = new List<StaffSpecializationLink>();
    public virtual ICollection<StaffCompensation> StaffCompensations { get; set; } = new List<StaffCompensation>();
    public virtual ICollection<StaffCompensationHistory> StaffCompensationHistories { get; set; } = new List<StaffCompensationHistory>();
}
