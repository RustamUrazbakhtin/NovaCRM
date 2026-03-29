using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;
using NovaCRM.Server.Contracts.Staff;
using NovaCRM.Server.Controllers;
using NovaCRM.Server.Services;

namespace NovaCRM.Server.Tests.Staff;

public class StaffControllerTests
{
    [Fact]
    public async Task Create_And_Update_Staff_With_Multiple_Roles_And_Specializations()
    {
        var orgId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new ApplicationDbContext(options);
        db.Organizations.Add(new Organization { Id = orgId, Name = "Org", Timezone = "UTC", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.StaffRoles.AddRange(
            new StaffRole { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Admin", Code = "admin" },
            new StaffRole { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Specialist", Code = "specialist" });
        db.StaffSpecializations.AddRange(
            new StaffSpecialization { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Lashes", Code = "lashes", IsActive = true },
            new StaffSpecialization { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Brows", Code = "brows", IsActive = true },
            new StaffSpecialization { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Laser", Code = "laser", IsActive = true });
        await db.SaveChangesAsync();

        var controller = new StaffController(db, new FakeOrgContext(orgId))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) } }
        };

        var roles = db.StaffRoles.Select(x => x.Id).ToArray();
        var specs = db.StaffSpecializations.Take(2).Select(x => x.Id).ToArray();
        var create = new UpsertStaffRequest(null, null, "A", "B", "+1", "a@b.com", null, true, "Available", 4.5m, 10, roles, specs,
            new StaffCompensationDto("Fixed", 3000, null, null, null, DateTime.UtcNow, null, null));

        var createResult = await controller.Create(create, CancellationToken.None);
        var created = Assert.IsType<OkObjectResult>(createResult.Result).Value as StaffDetailsDto;
        Assert.NotNull(created);
        Assert.Equal(2, created.Roles.Count);
        Assert.Equal(2, created.Specializations.Count);

        var updatedSpecs = db.StaffSpecializations.Skip(1).Select(x => x.Id).ToArray();
        var updateRequest = create with { SpecializationIds = updatedSpecs, Compensation = new StaffCompensationDto("Hybrid", 3200, null, 15, null, DateTime.UtcNow, null, "raise") };
        var updateResult = await controller.Update(created.Id, updateRequest, CancellationToken.None);
        var updated = Assert.IsType<OkObjectResult>(updateResult.Result).Value as StaffDetailsDto;

        Assert.NotNull(updated);
        Assert.Equal(2, updated.Specializations.Count);
        Assert.Equal("Hybrid", updated.CurrentCompensation?.CompensationType);
        Assert.True(updated.CompensationHistory.Count >= 2);
    }

    private sealed class FakeOrgContext(Guid orgId) : IOrganizationContext
    {
        public Task<Guid?> GetOrganizationIdAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(orgId);
    }
}
