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
                c.TotalVisits
            })
            .ToListAsync(cancellationToken);

        if (clients.Count == 0)
        {
            return Array.Empty<ClientRecord>();
        }

        var clientIds = clients.Select(c => c.Id).ToList();

        var satisfaction = await _dbContext.Reviews
            .AsNoTracking()
            .Where(r => r.DeletedAt == null
                && r.OrganizationId == organizationId
                && r.ClientId != null
                && clientIds.Contains(r.ClientId.Value))
            .GroupBy(r => r.ClientId!.Value)
            .Select(group => new
            {
                ClientId = group.Key,
                Avg = group.Average(r => (decimal?)r.Rating) ?? 0m
            })
            .ToListAsync(cancellationToken);

        var satisfactionByClient = satisfaction.ToDictionary(item => item.ClientId, item => item.Avg);

        var tags = await _dbContext.ClientTagLinks
            .AsNoTracking()
            .Where(link =>
                link.OrganizationId == organizationId
                && link.DeletedAt == null
                && clientIds.Contains(link.ClientId))
            .Join(
                _dbContext.ClientTags.AsNoTracking().Where(tag => tag.OrganizationId == organizationId && tag.DeletedAt == null),
                link => link.ClientTagId,
                tag => tag.Id,
                (link, tag) => new
                {
                    link.ClientId,
                    Tag = new ClientTag(tag.Id, tag.Name, tag.Color)
                })
            .ToListAsync(cancellationToken);

        var tagsByClient = tags
            .GroupBy(item => item.ClientId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<ClientTag>)group
                    .OrderBy(item => item.Tag.Name)
                    .Select(item => item.Tag)
                    .ToList());

        var metrics = await _dbContext.ClientMetrics
            .AsNoTracking()
            .Where(metric => metric.OrganizationId == organizationId && clientIds.Contains(metric.ClientId))
            .Select(metric => new
            {
                metric.ClientId,
                metric.LastVisitAt,
                metric.LifetimeValue,
                metric.Status
            })
            .ToListAsync(cancellationToken);

        var metricsByClient = metrics.ToDictionary(item => item.ClientId, item => item);

        return clients.Select(client =>
        {
            tagsByClient.TryGetValue(client.Id, out var clientTags);
            metricsByClient.TryGetValue(client.Id, out var metric);
            var clientSatisfaction = satisfactionByClient.TryGetValue(client.Id, out var score) ? score : 0m;

            return new ClientRecord(
                client.Id,
                client.FirstName,
                client.LastName,
                client.Phone,
                client.Email,
                clientTags ?? Array.Empty<ClientTag>(),
                metric?.LastVisitAt,
                metric?.LifetimeValue,
                metric?.Status,
                client.TotalVisits,
                clientSatisfaction
            );
        }).ToList();
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
                c.LastVisitAt,
                c.TotalVisits,
                c.Ltv,
                c.Notes,
                Tags = c.ClientTagLinks
                    .Where(link => link.OrganizationId == organizationId && link.DeletedAt == null)
                    .OrderBy(link => link.Tag.Name)
                    .Select(link => new ClientTag(link.ClientTagId, link.Tag.Name, link.Tag.Color)),
                City = c.Branch != null ? c.Branch.City : null,
                Master = c.Appointments
                    .Where(a => a.DeletedAt == null)
                    .OrderByDescending(a => a.StartAt)
                    .Select(a => a.Staff.FirstName + " " + a.Staff.LastName)
                    .FirstOrDefault(),
                Satisfaction = c.Reviews
                    .Where(r => r.DeletedAt == null)
                    .Select(r => (decimal?)r.Rating)
                    .DefaultIfEmpty(0m)
                    .Average() ?? 0m,
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
        Data.Model.ClientTag? segmentTag = null;
        if (request.SegmentTagId is not null)
        {
            segmentTag = await _dbContext.ClientTags
                .AsNoTracking()
                .Where(t => t.OrganizationId == organizationId && t.DeletedAt == null)
                .FirstOrDefaultAsync(t => t.Id == request.SegmentTagId.Value, cancellationToken);

            if (segmentTag is null)
            {
                throw new ArgumentException("Segment tag is not valid for this organization.");
            }
        }

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
            Segment = segmentTag?.Name,
            MarketingOptIn = false,
            TotalVisits = 0,
            Ltv = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Clients.Add(client);

        if (segmentTag is not null)
        {
            _dbContext.ClientTagLinks.Add(new Data.Model.ClientTagLink
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                ClientTagId = segmentTag.Id,
                OrganizationId = organizationId,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ClientCreatedResult(client.Id);
    }

    public async Task<IReadOnlyCollection<ClientTag>> GetTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var tags = await _dbContext.ClientTags
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId && t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .Select(t => new ClientTag(t.Id, t.Name, t.Color))
            .ToListAsync(cancellationToken);

        return tags;
    }

    public async Task<IReadOnlyCollection<ClientStatusTag>> GetStatusTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var tags = await _dbContext.ClientTags
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId && t.DeletedAt == null)
            .Where(t => ClientStatusRules.IsStatusName(t.Name))
            .OrderBy(t => t.Name)
            .Select(t => new ClientStatusTag(t.Id, t.Name, t.Color))
            .ToListAsync(cancellationToken);

        return tags;
    }
}
