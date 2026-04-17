using System.Text.RegularExpressions;
using NovaCRM.Domain.Staff.Model;

namespace NovaCRM.Domain.Staff.Services;

public sealed class StaffService : IStaffService
{
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public StaffItem BuildForUpsert(StaffUpsertInput input, Guid? existingId = null)
    {
        if (string.IsNullOrWhiteSpace(input.FirstName)) throw new StaffValidationException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(input.LastName)) throw new StaffValidationException("LastName is required.");
        if (!Enum.IsDefined(typeof(StaffStatusEnum), input.Status)) throw new StaffValidationException("Status must be a valid enum value.");
        if (!Enum.IsDefined(typeof(CompensationTypeEnum), input.CompensationType)) throw new StaffValidationException("CompensationType must be a valid enum value.");

        var email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email)) throw new StaffValidationException("Email format is invalid.");

        var compensationType = (CompensationTypeEnum)input.CompensationType;
        var fixedSalary = compensationType == CompensationTypeEnum.FixedSalary ? input.FixedSalary : null;
        var hourlyRate = compensationType == CompensationTypeEnum.HourlyRate ? input.HourlyRate : null;
        var commissionPercent = compensationType == CompensationTypeEnum.Commission ? input.CommissionPercent : null;

        ValidateCompensation(compensationType, fixedSalary, hourlyRate, commissionPercent);

        return new StaffItem
        {
            Id = existingId ?? Guid.NewGuid(),
            BranchId = input.BranchId,
            HasCrmAccess = input.HasCrmAccess,
            UserId = input.HasCrmAccess ? NormalizeNullable(input.UserId) : null,
            FirstName = input.FirstName.Trim(),
            LastName = input.LastName.Trim(),
            Phone = NormalizeNullable(input.Phone),
            Email = email,
            Notes = NormalizeNullable(input.Notes),
            IsActive = input.IsActive,
            Status = (StaffStatusEnum)input.Status,
            CompensationType = compensationType,
            FixedSalary = fixedSalary,
            HourlyRate = hourlyRate,
            CommissionPercent = commissionPercent,
            RatingAverage = input.RatingAverage,
            RatingCount = input.RatingCount,
            RoleIds = input.RoleIds?.Distinct().ToArray() ?? Array.Empty<Guid>(),
            SpecializationIds = input.SpecializationIds?.Distinct().ToArray() ?? Array.Empty<Guid>()
        };
    }

    public IReadOnlyCollection<EnumOption> GetStatusOptions() =>
        Enum.GetValues<StaffStatusEnum>().Select(x => new EnumOption((int)x, GetStatusName(x))).ToArray();

    public IReadOnlyCollection<EnumOption> GetCompensationTypeOptions() =>
        Enum.GetValues<CompensationTypeEnum>().Select(x => new EnumOption((int)x, GetCompensationTypeName(x))).ToArray();

    public string GetStatusName(StaffStatusEnum status) => status switch
    {
        StaffStatusEnum.Active => "Active",
        StaffStatusEnum.Vacation => "Vacation",
        StaffStatusEnum.Terminated => "Terminated",
        _ => status.ToString()
    };

    public string GetCompensationTypeName(CompensationTypeEnum compensationType) => compensationType switch
    {
        CompensationTypeEnum.FixedSalary => "Fixed salary",
        CompensationTypeEnum.HourlyRate => "Hourly rate",
        CompensationTypeEnum.Commission => "Commission",
        _ => compensationType.ToString()
    };

    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateCompensation(CompensationTypeEnum compensationType, decimal? fixedSalary, decimal? hourlyRate, decimal? commissionPercent)
    {
        switch (compensationType)
        {
            case CompensationTypeEnum.FixedSalary:
                if (fixedSalary is null) throw new StaffValidationException("FixedSalary is required for FixedSalary compensation type.");
                if (fixedSalary < 0) throw new StaffValidationException("FixedSalary cannot be negative.");
                return;
            case CompensationTypeEnum.HourlyRate:
                if (hourlyRate is null) throw new StaffValidationException("HourlyRate is required for HourlyRate compensation type.");
                if (hourlyRate < 0) throw new StaffValidationException("HourlyRate cannot be negative.");
                return;
            case CompensationTypeEnum.Commission:
                if (commissionPercent is null) throw new StaffValidationException("CommissionPercent is required for Commission compensation type.");
                if (commissionPercent < 0 || commissionPercent > 100) throw new StaffValidationException("CommissionPercent must be between 0 and 100.");
                return;
        }
    }
}
