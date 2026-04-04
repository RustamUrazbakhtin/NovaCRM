using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using NovaCRM.Data;
using NovaCRM.Data.Model;
using NovaCRM.Server.Contracts.Staff;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IOrganizationContext _organizationContext;

    public StaffController(ApplicationDbContext db, IOrganizationContext organizationContext)
    {
        _db = db;
        _organizationContext = organizationContext;
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

        return Ok(new StaffCatalogDto(roles, specializations, branches, users));
    }

    [HttpGet]
    public async Task<ActionResult<StaffListResponseDto>> Get(
     [FromQuery] string? search,
     [FromQuery] string? filter = "all",
     CancellationToken cancellationToken = default)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null)
        {
            return Ok(new StaffListResponseDto(
                new StaffOverviewDto(0, 0, 0, 0, 0, 0),
                Array.Empty<StaffListItemDto>()));
        }

        var now = DateTime.UtcNow;
        var dayStart = now.Date;
        var dayEnd = dayStart.AddDays(1);
        var weekEnd = dayStart.AddDays(7);

        IQueryable<Staff> query = _db.Staff
            .AsNoTracking()
            .Where(s => s.OrganizationId == orgId && s.DeletedAt == null)
            .Include(s => s.Branch)
            .Include(s => s.StaffRoleLinks)
                .ThenInclude(x => x.Role)
            .Include(s => s.StaffSpecializationLinks)
                .ThenInclude(x => x.Specialization)
            .Include(s => s.StaffCompensations)
            .Include(s => s.Appointments.Where(a =>
                a.DeletedAt == null &&
                a.StartAt >= dayStart &&
                a.StartAt < weekEnd));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            var pattern = $"%{normalized}%";

            query = query.Where(s =>
                EF.Functions.ILike(((s.FirstName ?? "") + " " + (s.LastName ?? "")).Trim(), pattern) ||
                EF.Functions.ILike(s.Phone ?? "", pattern) ||
                EF.Functions.ILike(s.Email ?? "", pattern) ||
                s.StaffRoleLinks.Any(r =>
                    r.Role != null &&
                    EF.Functions.ILike(r.Role.Name, pattern)) ||
                s.StaffSpecializationLinks.Any(sp =>
                    sp.Specialization != null &&
                    EF.Functions.ILike(sp.Specialization.Name, pattern))
            );
        }

        var list = await query.ToListAsync(cancellationToken);

        var response = list.Select(s =>
        {
            var apptsToday = s.Appointments.Count(a => a.StartAt >= dayStart && a.StartAt < dayEnd);
            var apptsWeek = s.Appointments.Count;

            var currentComp = s.StaffCompensations
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault(x =>
                    x.EffectiveFrom <= now &&
                    (x.EffectiveTo == null || x.EffectiveTo >= now));

            return new StaffListItemDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.Phone,
                s.Email,
                s.IsActive,
                s.EmploymentStatus,
                s.RatingAverage,
                s.RatingCount,
                s.BranchId,
                s.Branch?.Name,
                string.Equals(s.EmploymentStatus, "OnLeave", StringComparison.OrdinalIgnoreCase)
                    ? "On leave"
                    : (apptsToday > 0 ? $"Busy today ({apptsToday})" : "No appointments today"),
                apptsToday,
                apptsWeek,
                s.HasCrmAccess,
                s.UserId,
                s.StaffRoleLinks
                    .Where(x => x.Role != null)
                    .Select(x => new StaffLookupDto(x.RoleId, x.Role.Name, x.Role.Code))
                    .ToList(),
                s.StaffSpecializationLinks
                    .Where(x => x.Specialization != null)
                    .Select(x => new StaffLookupDto(x.SpecializationId, x.Specialization.Name, x.Specialization.Code))
                    .ToList(),
                currentComp == null
                    ? null
                    : new StaffCompensationDto(
                        currentComp.CompensationType,
                        currentComp.FixedSalary,
                        currentComp.HourlyRate,
                        currentComp.CommissionPercent,
                        currentComp.PerServiceAmount,
                        currentComp.EffectiveFrom,
                        currentComp.EffectiveTo,
                        currentComp.Notes)
            );
        }).ToList();

        response = ApplyFilter(response, filter);

        var overview = new StaffOverviewDto(
            list.Count,
            list.Count(s => s.IsActive),
            list.Count(s => s.Appointments.Any(a => a.StartAt >= dayStart && a.StartAt < dayEnd)),
            list.Count(s => string.Equals(s.EmploymentStatus, "Available", StringComparison.OrdinalIgnoreCase)),
            list.Count > 0 ? Math.Round(list.Average(s => s.RatingAverage), 2) : 0,
            list.SelectMany(s => s.StaffCompensations)
                .Where(c => c.FixedSalary.HasValue)
                .Sum(c => c.FixedSalary ?? 0)
        );

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
            .Include(s => s.StaffCompensationHistories)
            .FirstOrDefaultAsync(cancellationToken);
        if (staff is null) return NotFound();

        var currentComp = staff.StaffCompensations.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
        var history = staff.StaffCompensations.OrderByDescending(x => x.EffectiveFrom)
            .Select(c => new StaffCompensationDto(c.CompensationType, c.FixedSalary, c.HourlyRate, c.CommissionPercent, c.PerServiceAmount, c.EffectiveFrom, c.EffectiveTo, c.Notes)).ToList();

        return Ok(new StaffDetailsDto(staff.Id, staff.BranchId, staff.HasCrmAccess, staff.UserId, staff.FirstName, staff.LastName, staff.Phone, staff.Email, staff.Notes,
            staff.IsActive, staff.EmploymentStatus, staff.RatingAverage, staff.RatingCount,
            staff.StaffRoleLinks.Select(x => new StaffLookupDto(x.RoleId, x.Role.Name, x.Role.Code)).ToList(),
            staff.StaffSpecializationLinks.Select(x => new StaffLookupDto(x.SpecializationId, x.Specialization.Name, x.Specialization.Code)).ToList(),
            currentComp is null ? null : new StaffCompensationDto(currentComp.CompensationType, currentComp.FixedSalary, currentComp.HourlyRate, currentComp.CommissionPercent, currentComp.PerServiceAmount, currentComp.EffectiveFrom, currentComp.EffectiveTo, currentComp.Notes), history));
    }

    [HttpPost]
    public async Task<ActionResult<StaffDetailsDto>> Create([FromBody] UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var now = DateTime.UtcNow;
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            BranchId = request.BranchId,
            HasCrmAccess = request.HasCrmAccess,
            UserId = request.HasCrmAccess && !string.IsNullOrWhiteSpace(request.UserId) ? request.UserId : null,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Notes = request.Notes?.Trim(),
            IsActive = request.IsActive,
            EmploymentStatus = request.EmploymentStatus,
            RatingAverage = request.RatingAverage ?? 0,
            RatingCount = request.RatingCount ?? 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Staff.Add(staff);
        await _db.SaveChangesAsync(cancellationToken);
        await UpsertLinksAndCompensationAsync(staff.Id, orgId.Value, request, cancellationToken);
        return await GetById(staff.Id, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffDetailsDto>> Update(Guid id, [FromBody] UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Id == id && s.DeletedAt == null, cancellationToken);
        if (staff is null) return NotFound();

        staff.BranchId = request.BranchId;
        staff.HasCrmAccess = request.HasCrmAccess;
        staff.UserId = request.HasCrmAccess && !string.IsNullOrWhiteSpace(request.UserId) ? request.UserId : null;
        staff.FirstName = request.FirstName.Trim();
        staff.LastName = request.LastName.Trim();
        staff.Phone = request.Phone?.Trim();
        staff.Email = request.Email?.Trim();
        staff.Notes = request.Notes?.Trim();
        staff.IsActive = request.IsActive;
        staff.EmploymentStatus = request.EmploymentStatus;
        staff.RatingAverage = request.RatingAverage ?? staff.RatingAverage;
        staff.RatingCount = request.RatingCount ?? staff.RatingCount;
        staff.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await UpsertLinksAndCompensationAsync(staff.Id, orgId.Value, request, cancellationToken);
        return await GetById(staff.Id, cancellationToken);
    }

    private static List<StaffListItemDto> ApplyFilter(List<StaffListItemDto> source, string? filter)
    {
        return filter?.ToLowerInvariant() switch
        {
            "active" => source.Where(x => x.IsActive).ToList(),
            "available" => source.Where(x => x.EmploymentStatus.Equals("Available", StringComparison.OrdinalIgnoreCase)).ToList(),
            "busy" => source.Where(x => x.EmploymentStatus.Equals("Busy", StringComparison.OrdinalIgnoreCase)).ToList(),
            "on-leave" => source.Where(x => x.EmploymentStatus.Equals("OnLeave", StringComparison.OrdinalIgnoreCase)).ToList(),
            "admin" => source.Where(x => x.Roles.Any(r => r.Code == "admin")).ToList(),
            "specialist" => source.Where(x => x.Roles.Any(r => r.Code == "specialist")).ToList(),
            "owner" => source.Where(x => x.Roles.Any(r => r.Code == "owner")).ToList(),
            "manager" => source.Where(x => x.Roles.Any(r => r.Code == "manager")).ToList(),
            "top-rated" => source.Where(x => x.RatingAverage >= 4.8m).ToList(),
            "upcoming" => source.Where(x => x.AppointmentsWeek > 0).ToList(),
            _ => source
        };
    }

    private async Task UpsertLinksAndCompensationAsync(Guid staffId, Guid orgId, UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var roleIds = await _db.StaffRoles.Where(r => r.OrganizationId == orgId && request.RoleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync(cancellationToken);
        var specIds = await _db.StaffSpecializations.Where(s => s.OrganizationId == orgId && request.SpecializationIds.Contains(s.Id)).Select(s => s.Id).ToListAsync(cancellationToken);

        var currentRoles = _db.StaffRoleLinks.Where(x => x.StaffId == staffId);
        _db.StaffRoleLinks.RemoveRange(currentRoles);
        _db.StaffRoleLinks.AddRange(roleIds.Select(id => new StaffRoleLink { StaffId = staffId, RoleId = id, CreatedAt = DateTime.UtcNow }));

        var currentSpecs = _db.StaffSpecializationLinks.Where(x => x.StaffId == staffId);
        _db.StaffSpecializationLinks.RemoveRange(currentSpecs);
        _db.StaffSpecializationLinks.AddRange(specIds.Select(id => new StaffSpecializationLink { StaffId = staffId, SpecializationId = id, CreatedAt = DateTime.UtcNow }));

        if (request.Compensation is not null)
        {
            var latest = await _db.StaffCompensations.Where(x => x.StaffId == staffId).OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync(cancellationToken);
            if (latest is null || latest.CompensationType != request.Compensation.CompensationType || latest.FixedSalary != request.Compensation.FixedSalary || latest.HourlyRate != request.Compensation.HourlyRate || latest.CommissionPercent != request.Compensation.CommissionPercent || latest.PerServiceAmount != request.Compensation.PerServiceAmount)
            {
                _db.StaffCompensations.Add(new StaffCompensation
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    CompensationType = request.Compensation.CompensationType,
                    FixedSalary = request.Compensation.FixedSalary,
                    HourlyRate = request.Compensation.HourlyRate,
                    CommissionPercent = request.Compensation.CommissionPercent,
                    PerServiceAmount = request.Compensation.PerServiceAmount,
                    EffectiveFrom = request.Compensation.EffectiveFrom == default ? DateTime.UtcNow : request.Compensation.EffectiveFrom,
                    EffectiveTo = request.Compensation.EffectiveTo,
                    Notes = request.Compensation.Notes,
                    CreatedAt = DateTime.UtcNow
                });

                _db.StaffCompensationHistories.Add(new StaffCompensationHistory
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    PreviousSnapshot = latest is null ? null : $"{latest.CompensationType}:{latest.FixedSalary}:{latest.HourlyRate}:{latest.CommissionPercent}:{latest.PerServiceAmount}",
                    NewSnapshot = $"{request.Compensation.CompensationType}:{request.Compensation.FixedSalary}:{request.Compensation.HourlyRate}:{request.Compensation.CommissionPercent}:{request.Compensation.PerServiceAmount}",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = User.Identity?.Name,
                    Notes = request.Compensation.Notes
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
