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
        try
        {
            var clients = await _dbContext.Clients
       .AsNoTracking()
       .Where(c => c.OrganizationId == organizationId && c.DeletedAt == null)
       .Select(c => new
       {
           c.Id,
           c.OrganizationId,
           c.BranchId,
           c.FirstName,
           c.LastName,
           c.Phone,
           c.Email,
           c.Segment,
           c.Notes,
           c.MarketingOptIn,
           c.LastVisitAt,
           c.TotalVisits,
           c.Ltv,
           c.CreatedAt,
           c.UpdatedAt,
           c.DeletedAt
       })
       .ToListAsync(cancellationToken);

            var clientIds = clients.Select(c => c.Id).ToList();

            var tags = await _dbContext.ClientTagLinks
                .AsNoTracking()
                .Where(link => link.ClientId != Guid.Empty && clientIds.Contains(link.ClientId))
                .Join(
                    _dbContext.ClientTags.AsNoTracking()
                        .Where(tag => tag.OrganizationId == organizationId && tag.DeletedAt == null),
                    link => link.TagId,
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
                    g => g.Key,
                    g => (IReadOnlyCollection<ClientTag>)g
                        .Select(x => x.Tag)
                        .DistinctBy(t => t.Id)
                        .ToList());

            return clients.Select(c =>
            {
                tagsByClient.TryGetValue(c.Id, out var clientTags);

                return new ClientRecord(
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.Phone,
                    c.Email,
                    clientTags ?? Array.Empty<ClientTag>(),
                    c.LastVisitAt,
                    c.Ltv,
                    c.TotalVisits > 1 ? "Returning" : "New",
                    c.TotalVisits,
                    0m
                );
            }).ToList();
        }
        catch (Exception ex)
        {
            // Log the exception (you can use your preferred logging framework)
            Console.WriteLine($"Error fetching clients: {ex.Message}");
            // Return an empty list or rethrow the exception based on your error handling strategy
            return Array.Empty<ClientRecord>();
        }
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
                City = c.Branch != null ? c.Branch.City : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (client is null)
        {
            return null;
        }

            var tags = await _dbContext.ClientTagLinks
        .AsNoTracking()
        .Where(link => link.ClientId == clientId)
        .Join(
            _dbContext.ClientTags
                .AsNoTracking()
                .Where(t => t.OrganizationId == organizationId && t.DeletedAt == null),
            link => link.TagId,
            tag => tag.Id,
            (_, tag) => new
            {
                tag.Id,
                tag.Name,
                tag.Color
            })
        .OrderBy(t => t.Name)
        .Select(t => new ClientTag(t.Id, t.Name, t.Color))
        .ToListAsync(cancellationToken);

            return new ClientDetailsRecord(
            client.Id,
            client.FirstName,
            client.LastName,
            client.Phone,
            client.Email,
            client.LastVisitAt,
            client.TotalVisits,
            client.Ltv,
            0m,
            tags,
            client.City,
            null,
            Array.Empty<ClientActivity>(),
            client.Notes);
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
            Notes = request.Notes,
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
                ClientId = client.Id,
                TagId = segmentTag.Id,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ClientCreatedResult(client.Id);
    }

    public async Task<bool> UpdateClientAsync(Guid organizationId, Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _dbContext.Clients
            .Where(c => c.OrganizationId == organizationId && c.Id == clientId && c.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (client is null)
        {
            return false;
        }

        client.FirstName = request.FirstName;
        client.LastName = request.LastName;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.Notes = request.Notes;
        client.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteClientAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _dbContext.Clients
            .Where(c => c.OrganizationId == organizationId && c.Id == clientId && c.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (client is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        client.DeletedAt = now;
        client.UpdatedAt = now;

        var links = await _dbContext.ClientTagLinks
            .Where(x => x.ClientId == clientId)
            .ToListAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetClientTagsAsync(Guid organizationId, Guid clientId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Clients
            .AnyAsync(c => c.OrganizationId == organizationId && c.Id == clientId && c.DeletedAt == null, cancellationToken);

        if (!exists)
        {
            return false;
        }

        var validTagIds = await _dbContext.ClientTags
            .Where(t => t.OrganizationId == organizationId && t.DeletedAt == null && tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var current = await _dbContext.ClientTagLinks
            .Where(x => x.ClientId == clientId)
            .ToListAsync(cancellationToken);

        var currentActiveIds = current.Select(x => x.TagId).ToHashSet();

        foreach (var tagId in validTagIds)
        {
            if (currentActiveIds.Contains(tagId))
            {
                continue;
            }

            var now = DateTime.UtcNow;

            _dbContext.ClientTagLinks.Add(new Data.Model.ClientTagLink
            {
                ClientId = clientId,
                TagId = tagId,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
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
