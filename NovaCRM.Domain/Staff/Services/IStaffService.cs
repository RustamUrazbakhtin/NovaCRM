using NovaCRM.Domain.Staff.Model;

namespace NovaCRM.Domain.Staff.Services;

public interface IStaffService
{
    StaffItem BuildForUpsert(StaffInsertInput input, Guid? existingId = null);
    IReadOnlyCollection<EnumOption> GetStatusOptions();
    IReadOnlyCollection<EnumOption> GetCompensationTypeOptions();
    string GetStatusName(StaffStatusEnum status);
    string GetCompensationTypeName(CompensationTypeEnum compensationType);
}
