namespace NovaCRM.Domain.Clients;

public record ClientOverview(
    int TotalClients,
    int ReturningClients,
    decimal AverageLtv,
    decimal Satisfaction
);
