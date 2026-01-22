using System;
using NovaCRM.Domain.Clients;

namespace NovaCRM.Server.Contracts.Clients;

public record ClientOverviewDto(int TotalClients, int ReturningClients, decimal AverageLtv, decimal Satisfaction)
{
    public static ClientOverviewDto FromDomain(ClientOverview overview) =>
        new(overview.TotalClients, overview.ReturningClients, overview.AverageLtv, overview.Satisfaction);
}

public record ClientListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    IReadOnlyCollection<ClientTagDto> Tags,
    DateTime? LastVisitAt,
    decimal? LifetimeValue,
    string? Status)
{
    public static ClientListItemDto FromDomain(ClientListItem item) =>
        new(
            item.Id,
            item.FirstName,
            item.LastName,
            item.Phone,
            item.Email,
            item.Tags.Select(ClientTagDto.FromDomain).ToList(),
            item.LastVisitAt,
            item.LifetimeValue,
            item.Status);
}

public record ClientDetailsDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? City,
    string? Master,
    string Status,
    string? StatusColor,
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
            details.Status,
            details.StatusColor,
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

public record CreateClientDto(string FirstName, string LastName, string Phone, string? Email, Guid? SegmentTagId)
{
    public CreateClientRequest ToDomain() => new(FirstName, LastName, Phone, Email, SegmentTagId);
}

public record ClientTagDto(Guid Id, string Name, string? Color)
{
    public static ClientTagDto FromDomain(ClientTag tag) => new(tag.Id, tag.Name, tag.Color);
}

public record ClientStatusTagDto(Guid Id, string Name, string? Color)
{
    public static ClientStatusTagDto FromDomain(ClientStatusTag tag) => new(tag.Id, tag.Name, tag.Color);
}

public record ClientFilterDto(string Key, string Label, string? Color)
{
    public static ClientFilterDto All { get; } = new("All", "All", null);

    public static ClientFilterDto FromDomain(ClientStatusTag tag) => new(tag.Id.ToString(), tag.Name, tag.Color);
}
