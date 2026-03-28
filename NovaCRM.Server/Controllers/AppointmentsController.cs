using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;
using NovaCRM.Server.Contracts.Appointments;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrganizationContext _organizationContext;

    public AppointmentsController(ApplicationDbContext dbContext, IOrganizationContext organizationContext)
    {
        _dbContext = dbContext;
        _organizationContext = organizationContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AppointmentListItemDto>>> GetAppointments(CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Ok(Array.Empty<AppointmentListItemDto>());
        }

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.OrganizationId == organizationId.Value && a.DeletedAt == null)
            .OrderByDescending(a => a.StartAt)
            .Select(a => new AppointmentListItemDto(
                a.Id,
                $"{a.Client.FirstName} {a.Client.LastName}".Trim(),
                a.Service.Name,
                a.Status,
                a.StartAt,
                a.PriceAtVisit,
                a.Notes))
            .ToListAsync(cancellationToken);

        return Ok(appointments);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentListItemDto>> CreateAppointment([FromBody] CreateAppointmentDto dto, CancellationToken cancellationToken = default)
    {
        var organizationId = await _organizationContext.GetOrganizationIdAsync(User, cancellationToken);
        if (organizationId is null)
        {
            return Unauthorized();
        }

        var client = await _dbContext.Clients
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId.Value && c.DeletedAt == null)
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId, cancellationToken);
        if (client is null)
        {
            return BadRequest(new { message = "Client does not exist for this organization." });
        }

        var service = await _dbContext.Services
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId.Value && s.DeletedAt == null && s.IsActive)
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId, cancellationToken);
        if (service is null)
        {
            return BadRequest(new { message = "Service does not exist for this organization." });
        }

        if (dto.EndsAt <= dto.StartsAt)
        {
            return BadRequest(new { message = "EndsAt must be after StartsAt." });
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            BranchId = client.BranchId,
            ClientId = client.Id,
            ServiceId = service.Id,
            StaffId = dto.StaffId,
            StartAt = dto.StartsAt,
            EndAt = dto.EndsAt,
            Status = "Scheduled",
            Source = "manual",
            PriceAtVisit = service.Price,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AppointmentListItemDto(
            appointment.Id,
            $"{client.FirstName} {client.LastName}".Trim(),
            service.Name,
            appointment.Status,
            appointment.StartAt,
            appointment.PriceAtVisit,
            appointment.Notes);

        return CreatedAtAction(nameof(GetAppointments), new { id = appointment.Id }, response);
    }
}
