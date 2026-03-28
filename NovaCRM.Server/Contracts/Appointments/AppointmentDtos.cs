namespace NovaCRM.Server.Contracts.Appointments;

public record CreateAppointmentDto(
    Guid ClientId,
    Guid ServiceId,
    Guid? StaffId,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Notes);

public record AppointmentListItemDto(
    Guid Id,
    string ClientName,
    string ServiceName,
    string Status,
    DateTime StartsAt,
    decimal PriceAtVisit,
    string? Notes);
