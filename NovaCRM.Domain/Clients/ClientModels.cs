namespace NovaCRM.Domain.Clients;

public record ClientListItem(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    ClientStatus Status,
    decimal LifetimeValue,
    DateTime? LastVisitAt,
    decimal Satisfaction,
    int Visits,
    IReadOnlyCollection<string> Tags,
    string? City
);

public record ClientDetails(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? City,
    string? Master,
    ClientStatus Status,
    decimal LifetimeValue,
    int Visits,
    decimal Satisfaction,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<ClientActivity> RecentActivity,
    string? Notes
);

public record ClientActivity(
    DateTime OccurredAt,
    string Title,
    string? Description
)
{
    public string OccurredAtLabel => OccurredAt.ToString("yyyy-MM-dd HH:mm");
}

public record CreateClientRequest(
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    string? Segment
);
