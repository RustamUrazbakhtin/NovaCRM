namespace NovaCRM.Domain.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
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
            FirstName = (request.FirstName ?? string.Empty).Trim(),
            LastName = (request.LastName ?? string.Empty).Trim(),
            Phone = (request.Phone ?? string.Empty).Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };

        ValidateClient(trimmed.FirstName, trimmed.LastName, trimmed.Phone);

        if (trimmed.SegmentTagId is not null)
        {
            return ValidateSegmentAndCreateAsync(organizationId, trimmed, cancellationToken);
        }

        return _repository.AddClientAsync(organizationId, trimmed, cancellationToken);
    }

    public Task<bool> UpdateClientAsync(Guid organizationId, Guid clientId, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var trimmed = request with
        {
            FirstName = (request.FirstName ?? string.Empty).Trim(),
            LastName = (request.LastName ?? string.Empty).Trim(),
            Phone = (request.Phone ?? string.Empty).Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        ValidateClient(trimmed.FirstName, trimmed.LastName, trimmed.Phone);

        return _repository.UpdateClientAsync(organizationId, clientId, trimmed, cancellationToken);
    }

    public Task<bool> DeleteClientAsync(Guid organizationId, Guid clientId, CancellationToken cancellationToken = default)
        => _repository.DeleteClientAsync(organizationId, clientId, cancellationToken);

    public Task<bool> SetClientTagsAsync(Guid organizationId, Guid clientId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default)
        => _repository.SetClientTagsAsync(organizationId, clientId, tagIds, cancellationToken);

    public Task<IReadOnlyCollection<ClientTag>> GetTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return _repository.GetTagsAsync(organizationId, cancellationToken);
    }

    public Task<IReadOnlyCollection<ClientStatusTag>> GetStatusTagsAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return _repository.GetStatusTagsAsync(organizationId, cancellationToken);
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

    private static void ValidateClient(string firstName, string lastName, string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Phone is required");
        }
    }
}
