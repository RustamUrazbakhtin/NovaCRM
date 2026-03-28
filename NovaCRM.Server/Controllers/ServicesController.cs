using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Server.Contracts.Services;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrganizationContext _organizationContext;

    public ServicesController(ApplicationDbContext dbContext, IOrganizationContext organizationContext)
    {
        _dbContext = dbContext;
        _organizationContext = organizationContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ServiceListItemDto>>> GetServices(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(Array.Empty<ServiceListItemDto>());
        }

        var services = await _dbContext.Services
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId.Value && s.DeletedAt == null && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceListItemDto(
                s.Id,
                s.Name,
                s.Category != null ? s.Category.Name : null,
                s.DurationMinutes,
                s.Price))
            .ToListAsync(cancellationToken);

        return Ok(services);
    }
}
