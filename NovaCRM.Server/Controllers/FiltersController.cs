using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaCRM.Server.Contracts.Filters;
using NovaCRM.Server.Contracts.Clients;
using NovaCRM.Server.Services;
using NovaCRM.Domain.Clients;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FiltersController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IOrganizationContext _organizationContext;

    public FiltersController(IClientService clientService, IOrganizationContext organizationContext)
    {
        _clientService = clientService;
        _organizationContext = organizationContext;
    }

    [HttpGet]
    public async Task<ActionResult<FiltersDto>> GetFilters(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(new FiltersDto(
                Array.Empty<ClientTagDto>(),
                Array.Empty<ClientStatusTagDto>(),
                Array.Empty<ClientTagDto>()));
        }

        var tags = await _clientService.GetTagsAsync(organizationId.Value, cancellationToken);
        var statuses = await _clientService.GetStatusTagsAsync(organizationId.Value, cancellationToken);

        return Ok(new FiltersDto(
            tags.Select(ClientTagDto.FromDomain).ToList(),
            statuses.Select(ClientStatusTagDto.FromDomain).ToList(),
            tags.Select(ClientTagDto.FromDomain).ToList()));
    }
}
