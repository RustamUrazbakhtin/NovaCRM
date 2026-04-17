namespace NovaCRM.Domain.Staff.Model;

public class StaffItem
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public bool HasCrmAccess { get; set; }
    public string? UserId { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }

    public bool IsActive { get; set; }
    public StaffStatusEnum Status { get; set; }

    public decimal? FixedSalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? CommissionPercent { get; set; }
    public CompensationTypeEnum CompensationType { get; set; }

    public decimal? RatingAverage { get; set; }
    public int? RatingCount { get; set; }

    public IReadOnlyCollection<Guid> RoleIds { get; set; } = Array.Empty<Guid>();
    public IReadOnlyCollection<Guid> SpecializationIds { get; set; } = Array.Empty<Guid>();
}
