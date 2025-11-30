namespace NovaCRM.Domain.Clients;

public interface IClientRepository
{
    Task<IReadOnlyCollection<ClientRecord>> GetClientsAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<ClientDetailsRecord?> GetClientDetailsAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientCreatedResult> AddClientAsync(Guid organizationId, CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClientTag>> GetTagsAsync(Guid organizationId, CancellationToken cancellationToken = default);
}

public record ClientRecord(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    string? Segment,
    DateTime? LastVisitAt,
    int TotalVisits,
    decimal LifetimeValue,
    decimal Satisfaction,
    IReadOnlyCollection<string> Tags,
    string? City,
    string? MasterName
);

public record ClientDetailsRecord(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    string? Segment,
    DateTime? LastVisitAt,
    int TotalVisits,
    decimal LifetimeValue,
    decimal Satisfaction,
    IReadOnlyCollection<string> Tags,
    string? City,
    string? MasterName,
    IReadOnlyCollection<ClientActivity> RecentActivity,
    string? Notes
);

public record ClientCreatedResult(Guid Id);
