namespace NovaCRM.Domain.Clients;

public interface IClientService
{
    Task<ClientOverview> GetOverviewAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClientListItem>> SearchClientsAsync(
        Guid organizationId,
        string? query,
        ClientFilter filter,
        CancellationToken cancellationToken = default);
    Task<ClientDetails?> GetClientDetailsAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientCreatedResult> AddClientAsync(Guid organizationId, CreateClientRequest request, CancellationToken cancellationToken = default);
}
