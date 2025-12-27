using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;

namespace NovaCRM.Server.Services;

public interface IOrganizationContext
{
    Task<Guid?> GetOrganizationIdAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
}

public class OrganizationContext : IOrganizationContext
{
    private readonly ApplicationDbContext _dbContext;

    public OrganizationContext(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> GetOrganizationIdAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var staffOrg = await _dbContext.Staff
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => (Guid?)s.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken);

        return staffOrg;
    }
}
