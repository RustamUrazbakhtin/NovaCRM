namespace NovaCRM.Server.Contracts.Staff;

public record StaffLookupDto(Guid Id, string Name, string Code);
public record EnumOptionDto(int Id, string Name);
public record StaffCompensationDto(int CompensationType, string CompensationTypeName, decimal? FixedSalary, decimal? HourlyRate, decimal? CommissionPercent, decimal? PerServiceAmount, DateTime EffectiveFrom, DateTime? EffectiveTo, string? Notes);
public record StaffListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    bool IsActive,
    int EmploymentStatus,
    string EmploymentStatusName,
    decimal RatingAverage,
    int RatingCount,
    Guid? BranchId,
    string? BranchName,
    string? TodaySchedule,
    int AppointmentsToday,
    int AppointmentsWeek,
    bool HasCrmAccess,
    string? UserId,
    IReadOnlyCollection<StaffLookupDto> Roles,
    IReadOnlyCollection<StaffLookupDto> Specializations,
    StaffCompensationDto? CurrentCompensation);

public record StaffOverviewDto(int TotalStaff, int ActiveToday, int BookedToday, int AvailableToday, decimal AvgRating, decimal PayrollThisMonth);

public record StaffListResponseDto(StaffOverviewDto Overview, IReadOnlyCollection<StaffListItemDto> Items);

public record UpsertStaffRequest(
    Guid? BranchId,
    bool HasCrmAccess,
    string? UserId,
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsActive,
    int Status,
    decimal? RatingAverage,
    int? RatingCount,
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<Guid> SpecializationIds,
    int CompensationType,
    decimal? FixedSalary,
    decimal? HourlyRate,
    decimal? CommissionPercent);

public record StaffDetailsDto(
    Guid Id,
    Guid? BranchId,
    bool HasCrmAccess,
    string? UserId,
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsActive,
    int EmploymentStatus,
    string EmploymentStatusName,
    decimal RatingAverage,
    int RatingCount,
    IReadOnlyCollection<StaffLookupDto> Roles,
    IReadOnlyCollection<StaffLookupDto> Specializations,
    StaffCompensationDto? CurrentCompensation,
    IReadOnlyCollection<StaffCompensationDto> CompensationHistory);

public record StaffCatalogDto(
    IReadOnlyCollection<StaffLookupDto> Roles,
    IReadOnlyCollection<StaffLookupDto> Specializations,
    IReadOnlyCollection<StaffLookupDto> Branches,
    IReadOnlyCollection<StaffLookupDto> Users,
    IReadOnlyCollection<EnumOptionDto> Statuses,
    IReadOnlyCollection<EnumOptionDto> CompensationTypes);
