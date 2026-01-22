namespace NovaCRM.Domain.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<ClientOverview> GetOverviewAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var clients = await _repository.GetClientsAsync(organizationId, cancellationToken);
        if (clients.Count == 0)
        {
            return new ClientOverview(0, 0, 0, 0);
        }

        var returning = clients.Count(c => c.TotalVisits > 1);
        var averageLtv = Math.Round(clients.Average(c => c.LifetimeValue ?? 0m), 0);
        var satisfaction = Math.Round(clients.Average(c => c.Satisfaction), 1);

        return new ClientOverview(clients.Count, returning, averageLtv, satisfaction);
    }

    public async Task<IReadOnlyCollection<ClientListItem>> SearchClientsAsync(
        Guid organizationId,
        string? query,
        Guid? statusTagId,
        CancellationToken cancellationToken = default)
    {
        var clients = await _repository.GetClientsAsync(organizationId, cancellationToken);
        var normalizedQuery = (query ?? string.Empty).Trim().ToLowerInvariant();

        var filtered = clients
            .Where(client => MatchesSearch(client, normalizedQuery))
            .Where(client => statusTagId is null || client.Tags.Any(tag => tag.Id == statusTagId))
            .Select(client => new ClientListItem(
                client.Id,
                client.FirstName,
                client.LastName,
                client.Phone,
                client.Email,
                client.Tags,
                client.LastVisitAt,
                client.LifetimeValue,
                client.Status
            ))
            .OrderByDescending(x => x.LastVisitAt ?? DateTime.MinValue)
            .ToList();

        return filtered;
    }

    public async Task<ClientDetails?> GetClientDetailsAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetClientDetailsAsync(organizationId, clientId, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var status = ResolveStatus(record.Tags);

        return new ClientDetails(
            record.Id,
            BuildName(record.FirstName, record.LastName),
            record.Phone,
            record.Email,
            record.City,
            record.MasterName,
            status.Name,
            status.Color,
            record.LifetimeValue,
            record.TotalVisits,
            record.Satisfaction,
            record.Tags.Select(t => t.Name).ToList(),
            record.RecentActivity,
            record.Notes
        );
    }

    public Task<ClientCreatedResult> AddClientAsync(Guid organizationId, CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var trimmed = request with
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
        };

        if (string.IsNullOrWhiteSpace(trimmed.FirstName))
        {
            throw new ArgumentException("First name is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(trimmed.LastName))
        {
            throw new ArgumentException("Last name is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(trimmed.Phone))
        {
            throw new ArgumentException("Phone is required", nameof(request));
        }

        if (trimmed.SegmentTagId is not null)
        {
            return ValidateSegmentAndCreateAsync(organizationId, trimmed, cancellationToken);
        }

        return _repository.AddClientAsync(organizationId, trimmed, cancellationToken);
    }

    public Task<IReadOnlyCollection<ClientTag>> GetTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return _repository.GetTagsAsync(organizationId, cancellationToken);
    }

    public Task<IReadOnlyCollection<ClientStatusTag>> GetStatusTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return _repository.GetStatusTagsAsync(organizationId, cancellationToken);
    }

    private static bool MatchesSearch(ClientRecord client, string normalizedQuery)
    {
        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return true;
        }

        var name = BuildName(client.FirstName, client.LastName).ToLowerInvariant();
        return name.Contains(normalizedQuery)
            || client.Phone.ToLowerInvariant().Contains(normalizedQuery)
            || (client.Email?.ToLowerInvariant().Contains(normalizedQuery) ?? false);
    }

    private static string BuildName(string first, string last)
    {
        return string.IsNullOrWhiteSpace(last) ? first : $"{first} {last}";
    }

    private static ClientStatusTag ResolveStatus(IReadOnlyCollection<ClientTag> tags)
    {
        var statusTags = tags.Where(tag => ClientStatusRules.IsStatusName(tag.Name)).ToList();

        if (statusTags.Count == 0)
        {
            return new ClientStatusTag(Guid.Empty, "Regular", null);
        }

        var prioritized = ClientStatusRules.StatusPriority
            .Select(priority => statusTags.FirstOrDefault(tag => string.Equals(tag.Name, priority, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(tag => tag is not null);

        var chosen = prioritized ?? statusTags.First();
        return new ClientStatusTag(chosen.Id, chosen.Name, chosen.Color);
    }

    private async Task<ClientCreatedResult> ValidateSegmentAndCreateAsync(
        Guid organizationId,
        CreateClientRequest trimmed,
        CancellationToken cancellationToken)
    {
        var tags = await _repository.GetTagsAsync(organizationId, cancellationToken);
        if (trimmed.SegmentTagId is not null && tags.All(tag => tag.Id != trimmed.SegmentTagId.Value))
        {
            throw new ArgumentException("The selected segment is invalid for this organization.", nameof(trimmed));
        }

        return await _repository.AddClientAsync(organizationId, trimmed, cancellationToken);
    }
}
