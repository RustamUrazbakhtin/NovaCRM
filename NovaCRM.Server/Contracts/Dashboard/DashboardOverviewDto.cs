namespace NovaCRM.Server.Contracts.Dashboard;

public sealed record DashboardOverviewDto(
    DashboardTodaySummaryDto TodaySummary,
    IReadOnlyCollection<DashboardUpcomingItemDto> UpcomingSoon,
    DashboardAnalyticsSummaryDto AnalyticsSummary,
    DashboardAccountingPreviewDto AccountingPreview,
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

public sealed record DashboardAnalyticsSummaryDto(
    IReadOnlyCollection<DashboardAnalyticsRingDto> Rings,
    bool IsPlaceholder);

public sealed record DashboardAnalyticsRingDto(
    string Key,
    string Label,
    int ValuePercent,
    int CurrentValue,
    int? TargetValue,
    string Description);

public sealed record DashboardAccountingPreviewDto(
    decimal RevenueThisMonth,
    decimal? PayrollThisMonth,
    decimal? PendingAmount,
    int? FormsConfiguredCount,
    string StatusNote);

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
