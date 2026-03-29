namespace NovaCRM.Data.Model;

public partial class StaffSpecialization
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<StaffSpecializationLink> StaffSpecializationLinks { get; set; } = new List<StaffSpecializationLink>();
}
