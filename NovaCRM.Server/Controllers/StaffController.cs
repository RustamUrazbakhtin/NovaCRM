using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;
using NovaCRM.Domain.Staff.Model;
using NovaCRM.Domain.Staff.Services;
using NovaCRM.Server.Contracts.Staff;
using NovaCRM.Server.Services;
using System.Security.Claims;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IOrganizationContext _organizationContext;
    private readonly IStaffService _staffService;

    public StaffController(ApplicationDbContext db, IOrganizationContext organizationContext, IStaffService staffService)
    {
        _db = db;
        _organizationContext = organizationContext;
        _staffService = staffService;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<StaffCatalogDto>> GetCatalog(CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var roles = await _db.StaffRoles.AsNoTracking().Where(x => x.OrganizationId == orgId).OrderBy(x => x.SortOrder)
            .Select(x => new StaffLookupDto(x.Id, x.Name, x.Code)).ToListAsync(cancellationToken);
        var specializations = await _db.StaffSpecializations.AsNoTracking().Where(x => x.OrganizationId == orgId && x.IsActive).OrderBy(x => x.SortOrder)
            .Select(x => new StaffLookupDto(x.Id, x.Name, x.Code)).ToListAsync(cancellationToken);
        var branches = await _db.Branches.AsNoTracking().Where(x => x.OrganizationId == orgId && x.DeletedAt == null).OrderBy(x => x.Name)
            .Select(x => new StaffLookupDto(x.Id, x.Name, x.Name)).ToListAsync(cancellationToken);

        var orgLinkedUserIds = await _db.Staff.AsNoTracking()
            .Where(x => x.OrganizationId == orgId && x.UserId != null && x.DeletedAt == null)
            .Select(x => x.UserId!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(currentUserId) && !orgLinkedUserIds.Contains(currentUserId))
        {
            orgLinkedUserIds.Add(currentUserId);
        }

        var users = await _db.AspNetUsers.AsNoTracking()
            .Where(x => orgLinkedUserIds.Contains(x.Id))
            .OrderBy(x => x.Email)
            .Select(x => new StaffLookupDto(Guid.Empty, x.Email ?? x.UserName ?? x.Id, x.Id))
            .ToListAsync(cancellationToken);

        return Ok(new StaffCatalogDto(
            roles,
            specializations,
            branches,
            users,
            _staffService.GetStatusOptions().Select(x => new EnumOptionDto(x.Id, x.Name)).ToArray(),
            _staffService.GetCompensationTypeOptions().Select(x => new EnumOptionDto(x.Id, x.Name)).ToArray()));
    }

    [HttpGet]
    public async Task<ActionResult<StaffListResponseDto>> Get([FromQuery] string? search, [FromQuery] string? filter = "all", CancellationToken cancellationToken = default)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null)
        {
            return Ok(new StaffListResponseDto(new StaffOverviewDto(0, 0, 0, 0, 0, 0), Array.Empty<StaffListItemDto>()));
        }

        var now = DateTime.UtcNow;
        var dayStart = now.Date;
        var dayEnd = dayStart.AddDays(1);
        var weekEnd = dayStart.AddDays(7);

        IQueryable<Staff> query = _db.Staff
            .AsNoTracking()
            .Where(s => s.OrganizationId == orgId && s.DeletedAt == null)
            .Include(s => s.Branch)
            .Include(s => s.StaffRoleLinks).ThenInclude(x => x.Role)
            .Include(s => s.StaffSpecializationLinks).ThenInclude(x => x.Specialization)
            .Include(s => s.StaffCompensations)
            .Include(s => s.Appointments.Where(a => a.DeletedAt == null && a.StartAt >= dayStart && a.StartAt < weekEnd));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(((s.FirstName ?? "") + " " + (s.LastName ?? "")).Trim(), pattern) ||
                EF.Functions.ILike(s.Phone ?? "", pattern) ||
                EF.Functions.ILike(s.Email ?? "", pattern) ||
                s.StaffRoleLinks.Any(r => r.Role != null && EF.Functions.ILike(r.Role.Name, pattern)) ||
                s.StaffSpecializationLinks.Any(sp => sp.Specialization != null && EF.Functions.ILike(sp.Specialization.Name, pattern)));
        }

        var list = await query.ToListAsync(cancellationToken);
        var response = list.Select(s =>
        {
            var apptsToday = s.Appointments.Count(a => a.StartAt >= dayStart && a.StartAt < dayEnd);
            var apptsWeek = s.Appointments.Count;
            var currentComp = s.StaffCompensations.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault(x => x.EffectiveFrom <= now && (x.EffectiveTo == null || x.EffectiveTo >= now));
            var statusName = _staffService.GetStatusName(s.EmploymentStatus);

            return new StaffListItemDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.Phone,
                s.Email,
                s.IsActive,
                (int)s.EmploymentStatus,
                statusName,
                s.RatingAverage,
                s.RatingCount,
                s.BranchId,
                s.Branch?.Name,
                s.EmploymentStatus == StaffStatusEnum.Vacation ? "On vacation" : (apptsToday > 0 ? $"Busy today ({apptsToday})" : "No appointments today"),
                apptsToday,
                apptsWeek,
                s.HasCrmAccess,
                s.UserId,
                s.StaffRoleLinks.Where(x => x.Role != null).Select(x => new StaffLookupDto(x.RoleId, x.Role.Name, x.Role.Code)).ToList(),
                s.StaffSpecializationLinks.Where(x => x.Specialization != null).Select(x => new StaffLookupDto(x.SpecializationId, x.Specialization.Name, x.Specialization.Code)).ToList(),
                currentComp == null ? null : ToCompensationDto(currentComp));
        }).ToList();

        response = ApplyFilter(response, filter);

        var overview = new StaffOverviewDto(
            list.Count,
            list.Count(s => s.IsActive && s.EmploymentStatus == StaffStatusEnum.Active),
            list.Count(s => s.Appointments.Any(a => a.StartAt >= dayStart && a.StartAt < dayEnd)),
            list.Count(s => s.IsActive && s.EmploymentStatus == StaffStatusEnum.Active && !s.Appointments.Any(a => a.StartAt >= dayStart && a.StartAt < dayEnd)),
            list.Count > 0 ? Math.Round(list.Average(s => s.RatingAverage), 2) : 0,
            list.SelectMany(s => s.StaffCompensations).Where(c => c.FixedSalary.HasValue).Sum(c => c.FixedSalary ?? 0));

        return Ok(new StaffListResponseDto(overview, response));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StaffDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var staff = await _db.Staff.AsNoTracking()
            .Where(s => s.OrganizationId == orgId && s.Id == id && s.DeletedAt == null)
            .Include(s => s.StaffRoleLinks).ThenInclude(x => x.Role)
            .Include(s => s.StaffSpecializationLinks).ThenInclude(x => x.Specialization)
            .Include(s => s.StaffCompensations)
            .FirstOrDefaultAsync(cancellationToken);

        if (staff is null) return NotFound();

        var currentComp = staff.StaffCompensations.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
        var history = staff.StaffCompensations.OrderByDescending(x => x.EffectiveFrom).Select(ToCompensationDto).ToList();

        return Ok(new StaffDetailsDto(
            staff.Id,
            staff.BranchId,
            staff.HasCrmAccess,
            staff.UserId,
            staff.FirstName,
            staff.LastName,
            staff.Phone,
            staff.Email,
            staff.Notes,
            staff.IsActive,
            (int)staff.EmploymentStatus,
            _staffService.GetStatusName(staff.EmploymentStatus),
            staff.RatingAverage,
            staff.RatingCount,
            staff.StaffRoleLinks.Select(x => new StaffLookupDto(x.RoleId, x.Role.Name, x.Role.Code)).ToList(),
            staff.StaffSpecializationLinks.Select(x => new StaffLookupDto(x.SpecializationId, x.Specialization.Name, x.Specialization.Code)).ToList(),
            currentComp is null ? null : ToCompensationDto(currentComp),
            history));
    }

    [HttpPost]
    public async Task<ActionResult<StaffDetailsDto>> Create([FromBody] UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        StaffItem item;
        try
        {
            item = _staffService.BuildForUpsert(ToDomainInput(request));
        }
        catch (StaffValidationException ex)
        {
            return BadRequest(ex.Message);
        }

        var now = DateTime.UtcNow;
        var staff = new Staff
        {
            Id = item.Id,
            OrganizationId = orgId.Value,
            BranchId = item.BranchId,
            HasCrmAccess = item.HasCrmAccess,
            UserId = item.UserId,
            FirstName = item.FirstName,
            LastName = item.LastName,
            Phone = item.Phone,
            Email = item.Email,
            Notes = item.Notes,
            IsActive = item.IsActive,
            EmploymentStatus = item.Status,
            RatingAverage = item.RatingAverage ?? 0,
            RatingCount = item.RatingCount ?? 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Staff.Add(staff);
        await _db.SaveChangesAsync(cancellationToken);
        await UpsertLinksAndCompensationAsync(staff.Id, orgId.Value, item, cancellationToken);
        return await GetById(staff.Id, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffDetailsDto>> Update(Guid id, [FromBody] UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Id == id && s.DeletedAt == null, cancellationToken);
        if (staff is null) return NotFound();

        StaffItem item;
        try
        {
            item = _staffService.BuildForUpsert(ToDomainInput(request), id);
        }
        catch (StaffValidationException ex)
        {
            return BadRequest(ex.Message);
        }

        staff.BranchId = item.BranchId;
        staff.HasCrmAccess = item.HasCrmAccess;
        staff.UserId = item.UserId;
        staff.FirstName = item.FirstName;
        staff.LastName = item.LastName;
        staff.Phone = item.Phone;
        staff.Email = item.Email;
        staff.Notes = item.Notes;
        staff.IsActive = item.IsActive;
        staff.EmploymentStatus = item.Status;
        staff.RatingAverage = item.RatingAverage ?? staff.RatingAverage;
        staff.RatingCount = item.RatingCount ?? staff.RatingCount;
        staff.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await UpsertLinksAndCompensationAsync(staff.Id, orgId.Value, item, cancellationToken);
        return await GetById(staff.Id, cancellationToken);
    }

    private static List<StaffListItemDto> ApplyFilter(List<StaffListItemDto> source, string? filter)
    {
        return filter?.ToLowerInvariant() switch
        {
            "active" => source.Where(x => x.IsActive && x.EmploymentStatus == (int)StaffStatusEnum.Active).ToList(),
            "available" => source.Where(x => x.IsActive && x.EmploymentStatus == (int)StaffStatusEnum.Active && x.AppointmentsToday == 0).ToList(),
            "busy" => source.Where(x => x.IsActive && x.EmploymentStatus == (int)StaffStatusEnum.Active && x.AppointmentsToday > 0).ToList(),
            "on-leave" => source.Where(x => x.EmploymentStatus == (int)StaffStatusEnum.Vacation).ToList(),
            "admin" => source.Where(x => x.Roles.Any(r => r.Code == "admin")).ToList(),
            "specialist" => source.Where(x => x.Roles.Any(r => r.Code == "specialist")).ToList(),
            "owner" => source.Where(x => x.Roles.Any(r => r.Code == "owner")).ToList(),
            "manager" => source.Where(x => x.Roles.Any(r => r.Code == "manager")).ToList(),
            "top-rated" => source.Where(x => x.RatingAverage >= 4.8m).ToList(),
            "upcoming" => source.Where(x => x.AppointmentsWeek > 0).ToList(),
            _ => source
        };
    }

    private async Task UpsertLinksAndCompensationAsync(Guid staffId, Guid orgId, StaffItem item, CancellationToken cancellationToken)
    {
        var roleIds = await _db.StaffRoles.Where(r => r.OrganizationId == orgId && item.RoleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync(cancellationToken);
        var specIds = await _db.StaffSpecializations.Where(s => s.OrganizationId == orgId && item.SpecializationIds.Contains(s.Id)).Select(s => s.Id).ToListAsync(cancellationToken);

        _db.StaffRoleLinks.RemoveRange(_db.StaffRoleLinks.Where(x => x.StaffId == staffId));
        _db.StaffRoleLinks.AddRange(roleIds.Select(id => new StaffRoleLink { StaffId = staffId, RoleId = id, CreatedAt = DateTime.UtcNow }));

        _db.StaffSpecializationLinks.RemoveRange(_db.StaffSpecializationLinks.Where(x => x.StaffId == staffId));
        _db.StaffSpecializationLinks.AddRange(specIds.Select(id => new StaffSpecializationLink { StaffId = staffId, SpecializationId = id, CreatedAt = DateTime.UtcNow }));

        var latest = await _db.StaffCompensations.Where(x => x.StaffId == staffId).OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync(cancellationToken);
        if (latest is null || latest.CompensationType != item.CompensationType || latest.FixedSalary != item.FixedSalary || latest.HourlyRate != item.HourlyRate || latest.CommissionPercent != item.CommissionPercent)
        {
            _db.StaffCompensations.Add(new StaffCompensation
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                CompensationType = item.CompensationType,
                FixedSalary = item.FixedSalary,
                HourlyRate = item.HourlyRate,
                CommissionPercent = item.CommissionPercent,
                PerServiceAmount = null,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = null,
                Notes = null,
                CreatedAt = DateTime.UtcNow
            });

            _db.StaffCompensationHistories.Add(new StaffCompensationHistory
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                PreviousSnapshot = latest is null ? null : $"{(int)latest.CompensationType}:{latest.FixedSalary}:{latest.HourlyRate}:{latest.CommissionPercent}:{latest.PerServiceAmount}",
                NewSnapshot = $"{(int)item.CompensationType}:{item.FixedSalary}:{item.HourlyRate}:{item.CommissionPercent}:",
                ChangedAt = DateTime.UtcNow,
                ChangedBy = User.Identity?.Name,
                Notes = null
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private StaffCompensationDto ToCompensationDto(StaffCompensation compensation)
        => new((int)compensation.CompensationType, _staffService.GetCompensationTypeName(compensation.CompensationType), compensation.FixedSalary, compensation.HourlyRate, compensation.CommissionPercent, null, compensation.EffectiveFrom, compensation.EffectiveTo, compensation.Notes);

    private static StaffUpsertInput ToDomainInput(UpsertStaffRequest request) => new()
    {
        BranchId = request.BranchId,
        HasCrmAccess = request.HasCrmAccess,
        UserId = request.UserId,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Phone = request.Phone,
        Email = request.Email,
        Notes = request.Notes,
        IsActive = request.IsActive,
        Status = request.Status,
        RatingAverage = request.RatingAverage,
        RatingCount = request.RatingCount,
        RoleIds = request.RoleIds,
        SpecializationIds = request.SpecializationIds,
        CompensationType = request.CompensationType,
        FixedSalary = request.FixedSalary,
        HourlyRate = request.HourlyRate,
        CommissionPercent = request.CommissionPercent
    };
}
