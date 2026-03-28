using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(new { totalClients = 0, returning = 0, averageLtv = 0m, satisfaction = 0m });
        }

        var clients = await _clientRepository.GetClientsAsync(organizationId.Value, cancellationToken);
        var returning = clients.Count(client => client.TotalVisits > 1);
        var totalClients = clients.Count;
        var averageLtv = clients.Count > 0 ? Math.Round(clients.Average(c => c.LifetimeValue ?? 0m), 0) : 0m;
        var satisfaction = clients.Count > 0 ? Math.Round(clients.Average(c => c.Satisfaction), 1) : 0m;

        return Ok(new { totalClients, returning, averageLtv, satisfaction });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClientListItemDto>>> GetClients(
        [FromQuery] string? search,
        [FromQuery] string? filter,
        CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(Array.Empty<ClientListItemDto>());
        }

        var clients = await _clientRepository.GetClientsAsync(organizationId.Value, cancellationToken);

        var normalizedSearch = search?.Trim().ToLowerInvariant();
        var filteredClients = clients
            .Where(client => string.IsNullOrWhiteSpace(normalizedSearch)
                || $"{client.FirstName} {client.LastName}".ToLowerInvariant().Contains(normalizedSearch)
                || client.Phone.ToLowerInvariant().Contains(normalizedSearch)
                || (client.Email?.ToLowerInvariant().Contains(normalizedSearch) ?? false));

        if (!string.IsNullOrWhiteSpace(filter) && !string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(filter, out var filterTagId))
            {
                return BadRequest(new { message = "Invalid filter id." });
            }

            filteredClients = filteredClients.Where(c => c.Tags.Any(t => t.Id == filterTagId));
        }

        var response = filteredClients
            .Select(client => new ClientListItemDto(
                client.Id,
                client.FirstName,
                client.LastName,
                client.Phone,
                client.Email,
                client.Tags.Select(ClientTagDto.FromDomain).ToList(),
                client.LastVisitAt,
                client.LifetimeValue,
                client.Status))
            .ToList();

        return Ok(response);
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
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientDto dto, CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _clientService.UpdateClientAsync(organizationId.Value, id, dto.ToDomain(), cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var deleted = await _clientService.DeleteClientAsync(organizationId.Value, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/tags")]
    public async Task<IActionResult> SetClientTags(Guid id, [FromBody] UpdateClientTagsDto dto, CancellationToken cancellationToken)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var updated = await _clientService.SetClientTagsAsync(organizationId.Value, id, dto.TagIds.Distinct().ToList(), cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}
