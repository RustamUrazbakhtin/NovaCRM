namespace NovaCRM.Data.Model;

public partial class StaffRoleLink
{
    public Guid StaffId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Staff Staff { get; set; } = null!;
    public virtual StaffRole Role { get; set; } = null!;
}
