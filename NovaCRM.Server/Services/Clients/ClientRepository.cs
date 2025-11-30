using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Domain.Clients;

namespace NovaCRM.Server.Services.Clients;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClientRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ClientRecord>> GetClientsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var clients = await _dbContext.Clients
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && c.DeletedAt == null)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Phone,
                c.Email,
                c.Segment,
                c.LastVisitAt,
                c.TotalVisits,
                c.Ltv,
                Tags = c.ClientTagLinks.Select(link => link.Tag.Name),
                City = c.Branch != null ? c.Branch.City : null,
                Satisfaction = c.Reviews
                    .Where(r => r.DeletedAt == null)
                    .Select(r => (decimal?)r.Rating)
                    .DefaultIfEmpty(0)
                    .Average(),
                Master = c.Appointments
                    .Where(a => a.DeletedAt == null)
                    .OrderByDescending(a => a.StartAt)
                    .Select(a => a.Staff.FirstName + " " + a.Staff.LastName)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return clients.Select(c => new ClientRecord(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Phone,
            c.Email,
            c.Segment,
            c.LastVisitAt,
            c.TotalVisits,
            c.Ltv,
            c.Satisfaction,
            c.Tags.ToList(),
            c.City,
            c.Master
        )).ToList();
    }

    public async Task<ClientDetailsRecord?> GetClientDetailsAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _dbContext.Clients
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId && c.Id == clientId && c.DeletedAt == null)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Phone,
                c.Email,
                c.Segment,
                c.LastVisitAt,
                c.TotalVisits,
                c.Ltv,
                c.Notes,
                Tags = c.ClientTagLinks.Select(link => link.Tag.Name),
                City = c.Branch != null ? c.Branch.City : null,
                Master = c.Appointments
                    .Where(a => a.DeletedAt == null)
                    .OrderByDescending(a => a.StartAt)
                    .Select(a => a.Staff.FirstName + " " + a.Staff.LastName)
                    .FirstOrDefault(),
                Satisfaction = c.Reviews
                    .Where(r => r.DeletedAt == null)
                    .Select(r => (decimal?)r.Rating)
                    .DefaultIfEmpty(0)
                    .Average(),
                Activity = c.Appointments
                    .Where(a => a.DeletedAt == null)
                    .OrderByDescending(a => a.StartAt)
                    .Take(6)
                    .Select(a => new ClientActivity(
                        a.StartAt,
                        $"Visit · {a.Status}",
                        a.Branch != null ? a.Branch.Name : a.Notes
                    ))
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (client is null)
        {
            return null;
        }

        var activity = client.Activity.ToList();

        return new ClientDetailsRecord(
            client.Id,
            client.FirstName,
            client.LastName,
            client.Phone,
            client.Email,
            client.Segment,
            client.LastVisitAt,
            client.TotalVisits,
            client.Ltv,
            client.Satisfaction,
            client.Tags.ToList(),
            client.City,
            client.Master,
            activity,
            client.Notes
        );
    }

    public async Task<ClientCreatedResult> AddClientAsync(Guid organizationId, CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var branchId = await _dbContext.Branches
            .Where(b => b.OrganizationId == organizationId && b.DeletedAt == null)
            .OrderByDescending(b => b.IsDefault)
            .Select(b => (Guid?)b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var client = new Data.Model.Client
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            BranchId = branchId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            Segment = request.Segment,
            MarketingOptIn = false,
            TotalVisits = 0,
            Ltv = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ClientCreatedResult(client.Id);
    }
}
