using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
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

    private static readonly HashSet<string> AllowedEmploymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Active",
        "Vacation",
        "Terminated"
    };

    private static readonly HashSet<string> AllowedCompensationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "FixedSalary",
        "HourlyRate",
        "Commission"
    };

    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

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
                string.Equals(s.EmploymentStatus, "Vacation", StringComparison.OrdinalIgnoreCase)
                    ? "On vacation"
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
                currentComp == null ? null : ToCompensationDto(currentComp)
            );
        }).ToList();

        response = ApplyFilter(response, filter);

        var overview = new StaffOverviewDto(
            list.Count,
            list.Count(s => s.IsActive && s.EmploymentStatus.Equals("Active", StringComparison.OrdinalIgnoreCase)),
            list.Count(s => s.Appointments.Any(a => a.StartAt >= dayStart && a.StartAt < dayEnd)),
            list.Count(s => s.IsActive && s.EmploymentStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) && !s.Appointments.Any(a => a.StartAt >= dayStart && a.StartAt < dayEnd)),
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
        var history = staff.StaffCompensations
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(ToCompensationDto)
            .ToList();

        return Ok(new StaffDetailsDto(staff.Id, staff.BranchId, staff.HasCrmAccess, staff.UserId, staff.FirstName, staff.LastName, staff.Phone, staff.Email, staff.Notes,
            staff.IsActive, staff.EmploymentStatus, staff.RatingAverage, staff.RatingCount,
            staff.StaffRoleLinks.Select(x => new StaffLookupDto(x.RoleId, x.Role.Name, x.Role.Code)).ToList(),
            staff.StaffSpecializationLinks.Select(x => new StaffLookupDto(x.SpecializationId, x.Specialization.Name, x.Specialization.Code)).ToList(),
            currentComp is null ? null : ToCompensationDto(currentComp), history));
    }

    [HttpPost]
    public async Task<ActionResult<StaffDetailsDto>> Create([FromBody] UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var normalizedStatus = NormalizeEmploymentStatus(request.EmploymentStatus);
        if (normalizedStatus is null)
        {
            return BadRequest("EmploymentStatus must be one of: Active, Vacation, Terminated.");
        }

        var normalizedCompensationType = NormalizeCompensationType(request.CompensationType);
        if (normalizedCompensationType is null)
        {
            return BadRequest("CompensationType must be one of: FixedSalary, HourlyRate, Commission.");
        }

        var (fixedSalary, hourlyRate, commissionPercent) = NormalizeCompensationValues(normalizedCompensationType, request.FixedSalary, request.HourlyRate, request.CommissionPercent);
        var validationError = ValidateRequest(request, fixedSalary, hourlyRate, commissionPercent);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

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
            EmploymentStatus = normalizedStatus,
            RatingAverage = request.RatingAverage ?? 0,
            RatingCount = request.RatingCount ?? 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Staff.Add(staff);
        await _db.SaveChangesAsync(cancellationToken);
        await UpsertLinksAndCompensationAsync(staff.Id, orgId.Value, request, normalizedCompensationType, fixedSalary, hourlyRate, commissionPercent, cancellationToken);
        return await GetById(staff.Id, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffDetailsDto>> Update(Guid id, [FromBody] UpsertStaffRequest request, CancellationToken cancellationToken)
    {
        var orgId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (orgId is null) return Unauthorized();

        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Id == id && s.DeletedAt == null, cancellationToken);
        if (staff is null) return NotFound();

        var normalizedStatus = NormalizeEmploymentStatus(request.EmploymentStatus);
        if (normalizedStatus is null)
        {
            return BadRequest("EmploymentStatus must be one of: Active, Vacation, Terminated.");
        }

        var normalizedCompensationType = NormalizeCompensationType(request.CompensationType);
        if (normalizedCompensationType is null)
        {
            return BadRequest("CompensationType must be one of: FixedSalary, HourlyRate, Commission.");
        }

        var (fixedSalary, hourlyRate, commissionPercent) = NormalizeCompensationValues(normalizedCompensationType, request.FixedSalary, request.HourlyRate, request.CommissionPercent);
        var validationError = ValidateRequest(request, fixedSalary, hourlyRate, commissionPercent);
        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        staff.BranchId = request.BranchId;
        staff.HasCrmAccess = request.HasCrmAccess;
        staff.UserId = request.HasCrmAccess && !string.IsNullOrWhiteSpace(request.UserId) ? request.UserId : null;
        staff.FirstName = request.FirstName.Trim();
        staff.LastName = request.LastName.Trim();
        staff.Phone = request.Phone?.Trim();
        staff.Email = request.Email?.Trim();
        staff.Notes = request.Notes?.Trim();
        staff.IsActive = request.IsActive;
        staff.EmploymentStatus = normalizedStatus;
        staff.RatingAverage = request.RatingAverage ?? staff.RatingAverage;
        staff.RatingCount = request.RatingCount ?? staff.RatingCount;
        staff.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await UpsertLinksAndCompensationAsync(staff.Id, orgId.Value, request, normalizedCompensationType, fixedSalary, hourlyRate, commissionPercent, cancellationToken);
        return await GetById(staff.Id, cancellationToken);
    }

    private static List<StaffListItemDto> ApplyFilter(List<StaffListItemDto> source, string? filter)
    {
        return filter?.ToLowerInvariant() switch
        {
            "active" => source.Where(x => x.IsActive && x.EmploymentStatus.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToList(),
            "available" => source.Where(x => x.IsActive && x.EmploymentStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) && x.AppointmentsToday == 0).ToList(),
            "busy" => source.Where(x => x.IsActive && x.EmploymentStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) && x.AppointmentsToday > 0).ToList(),
            "on-leave" => source.Where(x => x.EmploymentStatus.Equals("Vacation", StringComparison.OrdinalIgnoreCase)).ToList(),
            "admin" => source.Where(x => x.Roles.Any(r => r.Code == "admin")).ToList(),
            "specialist" => source.Where(x => x.Roles.Any(r => r.Code == "specialist")).ToList(),
            "owner" => source.Where(x => x.Roles.Any(r => r.Code == "owner")).ToList(),
            "manager" => source.Where(x => x.Roles.Any(r => r.Code == "manager")).ToList(),
            "top-rated" => source.Where(x => x.RatingAverage >= 4.8m).ToList(),
            "upcoming" => source.Where(x => x.AppointmentsWeek > 0).ToList(),
            _ => source
        };
    }

    private async Task UpsertLinksAndCompensationAsync(
        Guid staffId,
        Guid orgId,
        UpsertStaffRequest request,
        string compensationType,
        decimal? fixedSalary,
        decimal? hourlyRate,
        decimal? commissionPercent,
        CancellationToken cancellationToken)
    {
        var requestRoleIds = request.RoleIds ?? Array.Empty<Guid>();
        var requestSpecIds = request.SpecializationIds ?? Array.Empty<Guid>();
        var roleIds = await _db.StaffRoles.Where(r => r.OrganizationId == orgId && requestRoleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync(cancellationToken);
        var specIds = await _db.StaffSpecializations.Where(s => s.OrganizationId == orgId && requestSpecIds.Contains(s.Id)).Select(s => s.Id).ToListAsync(cancellationToken);

        var currentRoles = _db.StaffRoleLinks.Where(x => x.StaffId == staffId);
        _db.StaffRoleLinks.RemoveRange(currentRoles);
        _db.StaffRoleLinks.AddRange(roleIds.Select(id => new StaffRoleLink { StaffId = staffId, RoleId = id, CreatedAt = DateTime.UtcNow }));

        var currentSpecs = _db.StaffSpecializationLinks.Where(x => x.StaffId == staffId);
        _db.StaffSpecializationLinks.RemoveRange(currentSpecs);
        _db.StaffSpecializationLinks.AddRange(specIds.Select(id => new StaffSpecializationLink { StaffId = staffId, SpecializationId = id, CreatedAt = DateTime.UtcNow }));

        var latest = await _db.StaffCompensations.Where(x => x.StaffId == staffId).OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync(cancellationToken);
        if (latest is null ||
            !string.Equals(latest.CompensationType, compensationType, StringComparison.OrdinalIgnoreCase) ||
            latest.FixedSalary != fixedSalary ||
            latest.HourlyRate != hourlyRate ||
            latest.CommissionPercent != commissionPercent)
        {
            _db.StaffCompensations.Add(new StaffCompensation
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                CompensationType = compensationType,
                FixedSalary = fixedSalary,
                HourlyRate = hourlyRate,
                CommissionPercent = commissionPercent,
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
                PreviousSnapshot = latest is null ? null : $"{latest.CompensationType}:{latest.FixedSalary}:{latest.HourlyRate}:{latest.CommissionPercent}:{latest.PerServiceAmount}",
                NewSnapshot = $"{compensationType}:{fixedSalary}:{hourlyRate}:{commissionPercent}:",
                ChangedAt = DateTime.UtcNow,
                ChangedBy = User.Identity?.Name,
                Notes = null
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeEmploymentStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalized = status.Trim();
        if (string.Equals(normalized, "OnLeave", StringComparison.OrdinalIgnoreCase))
        {
            normalized = "Vacation";
        }

        return AllowedEmploymentStatuses.Contains(normalized) ? AllowedEmploymentStatuses.Single(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase)) : null;
    }

    private static string? NormalizeCompensationType(string? compensationType)
    {
        if (string.IsNullOrWhiteSpace(compensationType))
        {
            return null;
        }

        var normalized = compensationType.Trim();
        normalized = normalized switch
        {
            "Fixed" => "FixedSalary",
            "Hourly" => "HourlyRate",
            _ => normalized
        };

        return AllowedCompensationTypes.Contains(normalized)
            ? AllowedCompensationTypes.Single(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            : null;
    }

    private static (decimal? FixedSalary, decimal? HourlyRate, decimal? CommissionPercent) NormalizeCompensationValues(string compensationType, decimal? fixedSalary, decimal? hourlyRate, decimal? commissionPercent)
    {
        return compensationType switch
        {
            "FixedSalary" => (fixedSalary, null, null),
            "HourlyRate" => (null, hourlyRate, null),
            "Commission" => (null, null, commissionPercent),
            _ => (null, null, null)
        };
    }

    private static StaffCompensationDto ToCompensationDto(StaffCompensation compensation)
    {
        var normalizedCompensationType = NormalizeCompensationType(compensation.CompensationType) ?? "FixedSalary";
        var (fixedSalary, hourlyRate, commissionPercent) = NormalizeCompensationValues(normalizedCompensationType, compensation.FixedSalary, compensation.HourlyRate, compensation.CommissionPercent);
        return new StaffCompensationDto(
            normalizedCompensationType,
            fixedSalary,
            hourlyRate,
            commissionPercent,
            null,
            compensation.EffectiveFrom,
            compensation.EffectiveTo,
            compensation.Notes);
    }

    private static string? ValidateRequest(UpsertStaffRequest request, decimal? fixedSalary, decimal? hourlyRate, decimal? commissionPercent)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return "FirstName is required.";
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return "LastName is required.";
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !EmailRegex.IsMatch(request.Email.Trim()))
        {
            return "Email format is invalid.";
        }

        var normalizedCompensationType = NormalizeCompensationType(request.CompensationType) ?? "FixedSalary";
        return normalizedCompensationType switch
        {
            "FixedSalary" when fixedSalary is null => "FixedSalary is required for FixedSalary compensation type.",
            "FixedSalary" when fixedSalary < 0 => "FixedSalary cannot be negative.",
            "HourlyRate" when hourlyRate is null => "HourlyRate is required for HourlyRate compensation type.",
            "HourlyRate" when hourlyRate < 0 => "HourlyRate cannot be negative.",
            "Commission" when commissionPercent is null => "CommissionPercent is required for Commission compensation type.",
            "Commission" when commissionPercent < 0 || commissionPercent > 100 => "CommissionPercent must be between 0 and 100.",
            _ => null
        };
    }
}
