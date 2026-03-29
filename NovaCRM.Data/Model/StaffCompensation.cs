namespace NovaCRM.Data.Model;

public partial class StaffCompensation
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string CompensationType { get; set; } = null!;
    public decimal? FixedSalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? CommissionPercent { get; set; }
    public decimal? PerServiceAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
