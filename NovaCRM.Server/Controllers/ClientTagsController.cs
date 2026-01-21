using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaCRM.Domain.Clients;
using NovaCRM.Server.Contracts.Clients;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/client-tags")]
[Authorize]
public class ClientTagsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IOrganizationContext _organizationContext;
    private readonly ILogger<ClientTagsController> _logger;

    public ClientTagsController(
        IClientService clientService,
        IOrganizationContext organizationContext,
        ILogger<ClientTagsController> logger)
    {
        _clientService = clientService;
        _organizationContext = organizationContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClientTagDto>>> GetClientTags(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            _logger.LogWarning(
                "Unable to load client tags because organization id is missing for user {UserId}.",
                User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Unauthorized();
        }

        try
        {
            var tags = await _clientService.GetTagsAsync(organizationId.Value, cancellationToken);
            return Ok(tags.Select(ClientTagDto.FromDomain).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load client tags for organization {OrganizationId}.", organizationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Failed to load client tags.",
                Detail = "An unexpected error occurred while loading client tags.",
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            });
        }
    }
}
