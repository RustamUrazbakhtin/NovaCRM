namespace NovaCRM.Data.Model;

public partial class StaffSpecializationLink
{
    public Guid StaffId { get; set; }
    public Guid SpecializationId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Staff Staff { get; set; } = null!;
    public virtual StaffSpecialization Specialization { get; set; } = null!;
}
