namespace NovaCRM.Server.Contracts.Services;

public record ServiceListItemDto(
    Guid Id,
    string Name,
    string? Category,
    int DurationMinutes,
    decimal Price);
