using System.Collections.Generic;
using NovaCRM.Server.Contracts.Clients;

namespace NovaCRM.Server.Contracts.Filters;

public record FiltersDto(
    IReadOnlyCollection<ClientTagDto> ClientTags,
    IReadOnlyCollection<ClientStatusTagDto> Statuses,
    IReadOnlyCollection<ClientTagDto> Segments);
