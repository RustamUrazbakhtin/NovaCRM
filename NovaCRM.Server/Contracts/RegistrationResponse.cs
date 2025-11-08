using System;

namespace NovaCRM.Server.Contracts;

public record RegistrationResponse(Guid OrganizationId, Guid BranchId, string OwnerUserId);
