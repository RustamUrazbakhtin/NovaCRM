using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private readonly IClientRepository _clientRepository;
    private readonly IOrganizationContext _organizationContext;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        IClientService clientService,
        IClientRepository clientRepository,
        IOrganizationContext organizationContext,
        ILogger<ClientsController> logger)
    {
        _clientService = clientService;
        _organizationContext = organizationContext;
        _logger = logger;
        _clientRepository = clientRepository;
    }

    //[HttpGet("overview")]
    //public async Task<ActionResult<ClientOverviewDto>> GetOverview(CancellationToken cancellationToken)
    //{
    //    var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
    //    if (organizationId is null)
    //    {
    //        _logger.LogWarning(
    //            "Unable to load clients because organization id is missing for user {UserId}.",
    //            User.FindFirstValue(ClaimTypes.NameIdentifier));
    //        return Unauthorized();
    //    }
    //
    //    var overview = await _clientService.GetOverviewAsync(organizationId.Value, cancellationToken);
    //    return Ok(ClientOverviewDto.FromDomain(overview));
    //}

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(new { totalClients = 0, returning = 0, averageLtv = 0m, satisfaction = 0m });
        }

        try
        {
            var clients = await _clientRepository.GetClientsAsync(organizationId.Value, cancellationToken);
            var returning = clients.Count(client => client.TotalVisits > 1);
            var totalClients = clients.Count;

            return Ok(new
            {
                totalClients,
                returning,
                averageLtv = 0m,
                satisfaction = 0m
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load client overview for organization {OrganizationId}.", organizationId);
            return Ok(new { totalClients = 0, returning = 0, averageLtv = 0m, satisfaction = 0m });
        }
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
            return Ok(Array.Empty<ClientListItemDto>());
        }

        //Guid? statusTagId = null;
        //if (!string.IsNullOrWhiteSpace(filter) && !string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
        //{
        //    if (!Guid.TryParse(filter, out var parsed))
        //    {
        //        return BadRequest(new ProblemDetails
        //        {
        //            Status = StatusCodes.Status400BadRequest,
        //            Title = "Invalid filter",
        //            Detail = "The filter must be \"All\" or a valid status tag identifier.",
        //            Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
        //        });
        //    }
        //
        //    statusTagId = parsed;
        //}

        try
        {
            var clients = await _clientRepository.GetClientsAsync(organizationId.Value, cancellationToken);
            //_clientService.SearchClientsAsync(organizationId.Value, search, statusTagId, cancellationToken);
            var response = clients
                .Select(client => new ClientListItemDto(
                    client.Id,
                    client.FirstName,
                    client.LastName,
                    client.Phone,
                    client.Email,
                    Array.Empty<ClientTagDto>(),
                    null,
                    null,
                    null))
                .ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load clients for organization {OrganizationId}.", organizationId);
            return Ok(Array.Empty<ClientListItemDto>());
        }
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
    public async Task<ActionResult<IReadOnlyCollection<ClientTag>>> GetFilters(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var tags = await _clientService.GetTagsAsync(organizationId.Value, cancellationToken);
        //var filters = new List<ClientTag> { ClientFilterDto.All };
        //filters.AddRange(tags.Select(ClientFilterDto.FromDomain));

        return Ok(tags);
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
