using NovaCRM.Domain.Clients;

namespace NovaCRM.Server.Contracts.Clients;

public record ClientOverviewDto(int TotalClients, int ReturningClients, decimal AverageLtv, decimal Satisfaction)
{
    public static ClientOverviewDto FromDomain(ClientOverview overview) =>
        new(overview.TotalClients, overview.ReturningClients, overview.AverageLtv, overview.Satisfaction);
}

public record ClientFilterDto(string Key, string Label)
{
    public static ClientFilterDto FromDomain(ClientFilterDefinition filter) => new(filter.Key, filter.Label);
}

public record ClientListItemDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string Status,
    decimal LifetimeValue,
    DateTime? LastVisitAt,
    decimal Satisfaction,
    int Visits,
    IReadOnlyCollection<string> Tags,
    string? City)
{
    public static ClientListItemDto FromDomain(ClientListItem item) =>
        new(
            item.Id,
            item.Name,
            item.Phone,
            item.Email,
            item.Status.ToString(),
            item.LifetimeValue,
            item.LastVisitAt,
            item.Satisfaction,
            item.Visits,
            item.Tags,
            item.City);
}

public record ClientDetailsDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? City,
    string? Master,
    string Status,
    decimal LifetimeValue,
    int Visits,
    decimal Satisfaction,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<ClientActivityDto> RecentActivity,
    string? Notes)
{
    public static ClientDetailsDto FromDomain(ClientDetails details) =>
        new(
            details.Id,
            details.Name,
            details.Phone,
            details.Email,
            details.City,
            details.Master,
            details.Status.ToString(),
            details.LifetimeValue,
            details.Visits,
            details.Satisfaction,
            details.Tags,
            details.RecentActivity.Select(ClientActivityDto.FromDomain).ToList(),
            details.Notes);
}

public record ClientActivityDto(DateTime OccurredAt, string Title, string? Description)
{
    public static ClientActivityDto FromDomain(ClientActivity activity) =>
        new(activity.OccurredAt, activity.Title, activity.Description);
}

public record CreateClientDto(string FirstName, string LastName, string Phone, string? Email, string? Segment)
{
    public CreateClientRequest ToDomain() => new(FirstName, LastName, Phone, Email, Segment);
}

public record ClientTagDto(Guid Id, string Name, string? Color)
{
    public static ClientTagDto FromDomain(ClientTag tag) => new(tag.Id, tag.Name, tag.Color);
}
