namespace NovaCRM.Domain.Clients;

public interface IClientService
{
    Task<ClientDetails?> GetClientDetailsAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientCreatedResult> AddClientAsync(Guid organizationId, CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateClientAsync(Guid organizationId, Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteClientAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default);
    Task<bool> SetClientTagsAsync(Guid organizationId, Guid clientId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClientTag>> GetTagsAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClientStatusTag>> GetStatusTagsAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
