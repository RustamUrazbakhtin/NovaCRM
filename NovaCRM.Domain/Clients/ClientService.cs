namespace NovaCRM.Domain.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    private const decimal VipLtvThreshold = 100000m;
    private static readonly TimeSpan AtRiskThreshold = TimeSpan.FromDays(90);

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
            .Select(client => new { client, status = ResolveStatus(client) })
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

        var status = ResolveStatus(record);

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

    private static ClientStatus ResolveStatus(ClientRecord record)
    {
        if (IsVip(record))
        {
            return ClientStatus.Vip;
        }

        if (IsAtRisk(record))
        {
            return ClientStatus.AtRisk;
        }

        if (record.TotalVisits <= 1)
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

    private static bool IsVip(ClientRecord client)
    {
        return string.Equals(client.Segment, "VIP", StringComparison.OrdinalIgnoreCase)
            || client.LifetimeValue >= VipLtvThreshold;
    }

    private static bool IsAtRisk(ClientRecord client)
    {
        if (client.LastVisitAt is null)
        {
            return true;
        }

        return client.LastVisitAt.Value < DateTime.UtcNow.Subtract(AtRiskThreshold);
    }

    private static string BuildName(string first, string last)
    {
        return string.IsNullOrWhiteSpace(last) ? first : $"{first} {last}";
    }
}
