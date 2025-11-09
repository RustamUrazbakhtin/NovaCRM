using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NovaCRM.Server.Contracts;

namespace NovaCRM.Server.Services;

public interface IRegistrationService
{
    Task<(bool Success, RegistrationResponse? Response, IEnumerable<string> Errors)> RegisterOrganizationAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default);
}
