namespace NovaCRM.Data.Model;

public partial class StaffCompensationHistory
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string? PreviousSnapshot { get; set; }
    public string NewSnapshot { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Notes { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
