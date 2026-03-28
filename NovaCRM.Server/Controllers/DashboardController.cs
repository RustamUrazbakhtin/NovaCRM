using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaCRM.Server.Contracts.Dashboard;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IOrganizationContext _organizationContext;
    private readonly IDashboardService _dashboardService;

    public DashboardController(IOrganizationContext organizationContext, IDashboardService dashboardService)
    {
        _organizationContext = organizationContext;
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(new DashboardOverviewDto(
                new DashboardTodaySummaryDto(0, 0, 0, 0),
                Array.Empty<DashboardUpcomingItemDto>(),
                new DashboardAnalyticsSummaryDto(Array.Empty<DashboardAnalyticsRingDto>(), true),
                new DashboardAccountingPreviewDto(0, null, null, null, "Accounting data not configured yet."),
                new DashboardStaffSummaryDto(0, 0, 0, 0, Array.Empty<DashboardStaffMemberDto>()),
                Array.Empty<DashboardCalendarCountDto>(),
                Array.Empty<DashboardRecentClientDto>(),
                Array.Empty<DashboardTagSummaryDto>(),
                new DashboardReviewSummaryDto(0, 0, 0, "flat")));
        }

        var overview = await _dashboardService.GetOverviewAsync(organizationId.Value, cancellationToken);
        return Ok(overview);
    }
}
