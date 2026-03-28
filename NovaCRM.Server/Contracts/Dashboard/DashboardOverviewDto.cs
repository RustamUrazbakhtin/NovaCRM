namespace NovaCRM.Server.Contracts.Dashboard;

public sealed record DashboardOverviewDto(
    DashboardTodaySummaryDto TodaySummary,
    IReadOnlyCollection<DashboardUpcomingItemDto> UpcomingAppointments,
    DashboardRevenueSummaryDto RevenueSummary,
    DashboardStaffSummaryDto StaffSummary,
    IReadOnlyCollection<DashboardCalendarCountDto> CalendarCounts,
    IReadOnlyCollection<DashboardRecentClientDto> RecentClients,
    IReadOnlyCollection<DashboardTagSummaryDto> ClientSegments,
    DashboardReviewSummaryDto ReviewsSummary);

public sealed record DashboardTodaySummaryDto(
    int AppointmentsToday,
    int NewClientsThisWeek,
    int NoShows,
    int CompletedVisitsToday);

public sealed record DashboardUpcomingItemDto(
    string Id,
    string StartTime,
    string ClientName,
    string? StaffName,
    string Status,
    string Date);

public sealed record DashboardRevenueSummaryDto(
    decimal CurrentMonthRevenue,
    decimal PreviousMonthRevenue,
    decimal GrowthPercent,
    string Trend);

public sealed record DashboardStaffSummaryDto(
    int Total,
    int InService,
    int OnBreak,
    int Available,
    IReadOnlyCollection<DashboardStaffMemberDto> Members);

public sealed record DashboardStaffMemberDto(
    string Id,
    string Name,
    string Status);

public sealed record DashboardCalendarCountDto(
    string Date,
    int Count);

public sealed record DashboardRecentClientDto(
    string Id,
    string Name,
    string Phone,
    string CreatedAt);

public sealed record DashboardTagSummaryDto(
    string Id,
    string Name,
    int Count);

public sealed record DashboardReviewSummaryDto(
    decimal AverageRating,
    int RecentCount,
    decimal PreviousPeriodAverage,
    string Trend);
