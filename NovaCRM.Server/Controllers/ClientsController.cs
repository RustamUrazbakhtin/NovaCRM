using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NovaCRM.Domain.Clients;
using NovaCRM.Server.Contracts.Clients;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IOrganizationContext _organizationContext;

    public ClientsController(IClientService clientService, IOrganizationContext organizationContext)
    {
        _clientService = clientService;
        _organizationContext = organizationContext;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ClientOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var overview = await _clientService.GetOverviewAsync(organizationId.Value, cancellationToken);
        return Ok(ClientOverviewDto.FromDomain(overview));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClientListItemDto>>> GetClients([
        FromQuery] string? search,
        [FromQuery] string? filter,
        CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        Guid? statusTagId = null;
        if (!string.IsNullOrWhiteSpace(filter) && !string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(filter, out var parsed))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid filter",
                    Detail = "The filter must be \"All\" or a valid status tag identifier.",
                    Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
                });
            }

            statusTagId = parsed;
        }

        var clients = await _clientService.SearchClientsAsync(organizationId.Value, search, statusTagId, cancellationToken);
        return Ok(clients.Select(ClientListItemDto.FromDomain).ToList());
    }

    [HttpGet("tags")]
    public async Task<ActionResult<IReadOnlyCollection<ClientTagDto>>> GetTags(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var tags = await _clientService.GetTagsAsync(organizationId.Value, cancellationToken);
        return Ok(tags.Select(ClientTagDto.FromDomain).ToList());
    }

    [HttpGet("status-tags")]
    public async Task<ActionResult<IReadOnlyCollection<ClientStatusTagDto>>> GetStatusTags(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var tags = await _clientService.GetStatusTagsAsync(organizationId.Value, cancellationToken);
        return Ok(tags.Select(ClientStatusTagDto.FromDomain).ToList());
    }

    [HttpGet("filters")]
    public async Task<ActionResult<IReadOnlyCollection<ClientFilterDto>>> GetFilters(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var tags = await _clientService.GetStatusTagsAsync(organizationId.Value, cancellationToken);
        var filters = new List<ClientFilterDto> { ClientFilterDto.All };
        filters.AddRange(tags.Select(ClientFilterDto.FromDomain));

        return Ok(filters);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDetailsDto>> GetClient(Guid id, CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var client = await _clientService.GetClientDetailsAsync(organizationId.Value, id, cancellationToken);
        if (client is null)
        {
            return NotFound();
        }

        return Ok(ClientDetailsDto.FromDomain(client));
    }

    [HttpPost]
    public async Task<ActionResult<ClientDetailsDto>> AddClient([FromBody] CreateClientDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        try
        {
            var created = await _clientService.AddClientAsync(organizationId.Value, dto.ToDomain(), cancellationToken);
            var client = await _clientService.GetClientDetailsAsync(organizationId.Value, created.Id, cancellationToken);
            if (client is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, ClientDetailsDto.FromDomain(client));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid client data",
                Detail = ex.Message,
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            });
        }
    }
}
