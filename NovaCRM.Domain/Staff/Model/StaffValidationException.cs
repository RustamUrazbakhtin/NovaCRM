namespace NovaCRM.Domain.Staff.Model;

public sealed class StaffValidationException(string message) : Exception(message);
