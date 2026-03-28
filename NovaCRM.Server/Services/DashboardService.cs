using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Server.Contracts.Dashboard;

namespace NovaCRM.Server.Services;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startOfToday = now.Date;
        var startOfTomorrow = startOfToday.AddDays(1);
        var startOfWeek = startOfToday.AddDays(-((int)startOfToday.DayOfWeek + 6) % 7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfPreviousMonth = startOfMonth.AddMonths(-1);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        var activeClients = _dbContext.Clients.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && c.DeletedAt == null);

        var activeStaff = _dbContext.Staff.AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && s.DeletedAt == null && s.IsActive);

        var appointmentsToday = await activeClients.CountAsync(c => c.LastVisitAt >= startOfToday && c.LastVisitAt < startOfTomorrow, cancellationToken);
        var newClientsThisWeek = await activeClients.CountAsync(c => c.CreatedAt >= startOfWeek && c.CreatedAt < startOfTomorrow, cancellationToken);
        var completedVisitsToday = appointmentsToday;

        var upcomingAppointments = await activeClients
            .Where(c => c.LastVisitAt >= now && c.LastVisitAt <= now.AddHours(2))
            .OrderBy(c => c.LastVisitAt)
            .Take(5)
            .Select(c => new DashboardUpcomingItemDto(
                c.Id.ToString(),
                (c.LastVisitAt ?? now).ToString("HH:mm"),
                string.Join(' ', new[] { c.FirstName, c.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
                null,
                "Scheduled",
                (c.LastVisitAt ?? now).ToString("yyyy-MM-dd")))
            .ToListAsync(cancellationToken);

        var monthlyRevenue = await activeClients
            .Where(c => c.LastVisitAt >= startOfMonth && c.LastVisitAt < startOfNextMonth)
            .Select(c => c.Ltv ?? 0m)
            .ToListAsync(cancellationToken);

        var previousMonthlyRevenue = await activeClients
            .Where(c => c.LastVisitAt >= startOfPreviousMonth && c.LastVisitAt < startOfMonth)
            .Select(c => c.Ltv ?? 0m)
            .ToListAsync(cancellationToken);

        var currentRevenue = monthlyRevenue.Sum();
        var previousRevenue = previousMonthlyRevenue.Sum();
        var growthPercent = previousRevenue <= 0
            ? (currentRevenue > 0 ? 100m : 0m)
            : Math.Round(((currentRevenue - previousRevenue) / previousRevenue) * 100m, 1);

        var trend = growthPercent switch
        {
            > 0.01m => "up",
            < -0.01m => "down",
            _ => "flat"
        };

        var staffMembers = await activeStaff
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new DashboardStaffMemberDto(
                s.Id.ToString(),
                $"{s.FirstName} {s.LastName}".Trim(),
                s.UserId != null ? "In service" : "Available"))
            .Take(4)
            .ToListAsync(cancellationToken);

        var staffTotal = await activeStaff.CountAsync(cancellationToken);
        var staffInService = await activeStaff.CountAsync(s => s.UserId != null, cancellationToken);
        var staffOnBreak = await activeStaff.CountAsync(s => s.UserId == null && (s.RoleTitle ?? string.Empty).Contains("break", StringComparison.OrdinalIgnoreCase), cancellationToken);
        var staffAvailable = Math.Max(0, staffTotal - staffInService - staffOnBreak);

        var calendarCounts = await activeClients
            .Where(c => c.LastVisitAt != null && c.LastVisitAt >= startOfMonth && c.LastVisitAt < startOfNextMonth)
            .GroupBy(c => c.LastVisitAt!.Value.Date)
            .Select(group => new DashboardCalendarCountDto(group.Key.ToString("yyyy-MM-dd"), group.Count()))
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var recentClients = await activeClients
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new DashboardRecentClientDto(
                c.Id.ToString(),
                $"{c.FirstName} {c.LastName}".Trim(),
                c.Phone,
                c.CreatedAt.ToString("yyyy-MM-dd")))
            .ToListAsync(cancellationToken);

        var clientSegments = await _dbContext.ClientTagLinks.AsNoTracking()
            .Where(link => link.OrganizationId == organizationId && link.DeletedAt == null)
            .GroupBy(link => link.ClientTagId)
            .Select(group => new
            {
                TagId = group.Key,
                Count = group.Select(x => x.ClientId).Distinct().Count()
            })
            .Join(_dbContext.ClientTags.AsNoTracking().Where(tag => tag.OrganizationId == organizationId && tag.DeletedAt == null),
                left => left.TagId,
                right => right.Id,
                (left, right) => new DashboardTagSummaryDto(right.Id.ToString(), right.Name, left.Count))
            .OrderByDescending(x => x.Count)
            .Take(4)
            .ToListAsync(cancellationToken);

        return new DashboardOverviewDto(
            new DashboardTodaySummaryDto(appointmentsToday, newClientsThisWeek, 0, completedVisitsToday),
            upcomingAppointments,
            new DashboardRevenueSummaryDto(currentRevenue, previousRevenue, growthPercent, trend),
            new DashboardStaffSummaryDto(staffTotal, staffInService, staffOnBreak, staffAvailable, staffMembers),
            calendarCounts,
            recentClients,
            clientSegments,
            new DashboardReviewSummaryDto(0m, 0, 0m, "flat"));
    }
}
