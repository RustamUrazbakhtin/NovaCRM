namespace NovaCRM.Domain.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    private const decimal VipLtvThreshold = 100000m;
    private static readonly TimeSpan AtRiskThreshold = TimeSpan.FromDays(90);
    private static readonly IReadOnlyCollection<ClientFilterDefinition> DefaultFilters = new[]
    {
        new ClientFilterDefinition(ClientFilter.All.ToString(), "All", 1),
        new ClientFilterDefinition(ClientFilter.Vip.ToString(), "VIP", 2),
        new ClientFilterDefinition(ClientFilter.Regular.ToString(), "Regular", 3),
        new ClientFilterDefinition(ClientFilter.New.ToString(), "New", 4),
        new ClientFilterDefinition(ClientFilter.AtRisk.ToString(), "At risk", 5)
    };

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
        var averageLtv = Math.Round(clients.Average(c => (decimal)c.LifetimeValue), 0);
        var satisfaction = Math.Round(clients.Average(c => c.Satisfaction), 1);

        return new ClientOverview(clients.Count, returning, averageLtv, satisfaction);
    }

    public async Task<IReadOnlyCollection<ClientListItem>> SearchClientsAsync(
        Guid organizationId,
        string? query,
        ClientFilter filter,
        CancellationToken cancellationToken = default)
    {
        var clients = await _repository.GetClientsAsync(organizationId, cancellationToken);
        var normalizedQuery = (query ?? string.Empty).Trim().ToLowerInvariant();

        var filtered = clients
            .Select(client => new
            {
                client,
                status = ResolveStatus(client.Segment, client.LifetimeValue, client.LastVisitAt, client.TotalVisits)
            })
            .Where(pair =>
                MatchesFilter(pair.status, filter) &&
                MatchesSearch(pair.client, normalizedQuery))
            .OrderByDescending(x => x.client.LastVisitAt ?? DateTime.MinValue)
            .Select(x => new ClientListItem(
                x.client.Id,
                BuildName(x.client.FirstName, x.client.LastName),
                x.client.Phone,
                x.client.Email,
                x.status,
                x.client.LifetimeValue,
                x.client.LastVisitAt,
                x.client.Satisfaction,
                x.client.TotalVisits,
                x.client.Tags,
                x.client.City
            ))
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

        var status = ResolveStatus(record.Segment, record.LifetimeValue, record.LastVisitAt, record.TotalVisits);

        return new ClientDetails(
            record.Id,
            BuildName(record.FirstName, record.LastName),
            record.Phone,
            record.Email,
            record.City,
            record.MasterName,
            status,
            record.LifetimeValue,
            record.TotalVisits,
            record.Satisfaction,
            record.Tags,
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
            Segment = string.IsNullOrWhiteSpace(request.Segment) ? null : request.Segment.Trim()
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

        return _repository.AddClientAsync(organizationId, trimmed, cancellationToken);
    }

    public Task<IReadOnlyCollection<ClientTag>> GetTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return _repository.GetTagsAsync(organizationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ClientFilterDefinition>> GetFiltersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var definitions = await _repository.GetFiltersAsync(organizationId, cancellationToken);

        var parsed = definitions
            .Select(definition => Enum.TryParse<ClientFilter>(definition.Key, true, out var parsedKey)
                ? new ClientFilterDefinition(parsedKey.ToString(), definition.Label, definition.SortOrder)
                : null)
            .Where(definition => definition is not null)
            .Select(definition => definition!)
            .OrderBy(definition => definition.SortOrder)
            .ToList();

        return parsed.Count > 0 ? parsed : DefaultFilters.ToList();
    }

    private static ClientStatus ResolveStatus(string? segment, decimal lifetimeValue, DateTime? lastVisitAt, int totalVisits)
    {
        if (IsVip(segment, lifetimeValue))
        {
            return ClientStatus.Vip;
        }

        if (IsAtRisk(lastVisitAt))
        {
            return ClientStatus.AtRisk;
        }

        if (totalVisits <= 1)
        {
            return ClientStatus.New;
        }

        return ClientStatus.Regular;
    }

    private static bool MatchesFilter(ClientStatus status, ClientFilter filter) => filter switch
    {
        ClientFilter.All => true,
        ClientFilter.Vip => status == ClientStatus.Vip,
        ClientFilter.Regular => status == ClientStatus.Regular,
        ClientFilter.New => status == ClientStatus.New,
        ClientFilter.AtRisk => status == ClientStatus.AtRisk,
        _ => true
    };

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

    private static bool IsVip(string? segment, decimal lifetimeValue)
    {
        return string.Equals(segment, "VIP", StringComparison.OrdinalIgnoreCase)
            || lifetimeValue >= VipLtvThreshold;
    }

    private static bool IsAtRisk(DateTime? lastVisitAt)
    {
        if (lastVisitAt is null)
        {
            return true;
        }

        return lastVisitAt.Value < DateTime.UtcNow.Subtract(AtRiskThreshold);
    }

    private static string BuildName(string first, string last)
    {
        return string.IsNullOrWhiteSpace(last) ? first : $"{first} {last}";
    }
}
