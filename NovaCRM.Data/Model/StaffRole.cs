namespace NovaCRM.Data.Model;

public partial class StaffRole
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsSystem { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<StaffRoleLink> StaffRoleLinks { get; set; } = new List<StaffRoleLink>();
}
