namespace NovaCRM.Domain.Staff.Model;

public class StaffInsertInput
{
    public Guid? BranchId { get; init; }
    public bool HasCrmAccess { get; init; }
    public string? UserId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
    public int Status { get; init; }
    public decimal? RatingAverage { get; init; }
    public int? RatingCount { get; init; }
    public IReadOnlyCollection<Guid>? RoleIds { get; init; }
    public IReadOnlyCollection<Guid>? SpecializationIds { get; init; }
    public int CompensationType { get; init; }
    public decimal? FixedSalary { get; init; }
    public decimal? HourlyRate { get; init; }
    public decimal? CommissionPercent { get; init; }
}
