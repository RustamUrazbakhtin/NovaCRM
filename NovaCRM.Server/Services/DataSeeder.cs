using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;

namespace NovaCRM.Server.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AspNetUser>>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.AspNetUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "NovaCRM Demo",
            Timezone = "UTC",
            Currency = "USD",
            PlanType = "free",
            SubscriptionStatus = "active",
            Phone = "+1 555 0100",
            Email = "owner@novacrm.demo",
            Settings = "{}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            Name = "Main location",
            City = "New York",
            Country = "US",
            Timezone = "UTC",
            IsDefault = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var user = new AspNetUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "owner@novacrm.demo",
            NormalizedUserName = "OWNER@NOVACRM.DEMO",
            Email = "owner@novacrm.demo",
            NormalizedEmail = "OWNER@NOVACRM.DEMO",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
            AccessFailedCount = 0
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "NovaCRM123!");

        var ownerRole = await db.AspNetRoles.FirstAsync(r => r.NormalizedName == "OWNER", cancellationToken);
        var adminRole = await db.AspNetRoles.FirstAsync(r => r.NormalizedName == "ADMIN", cancellationToken);
        user.Roles.Add(ownerRole);
        user.Roles.Add(adminRole);

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            BranchId = branch.Id,
            UserId = user.Id,
            FirstName = "Demo",
            LastName = "Owner",
            RoleTitle = "Owner",
            IsActive = true,
            Phone = "+1 555 0100",
            CreatedAt = now,
            UpdatedAt = now
        };

        var tags = new[]
        {
            new ClientTag { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "VIP", Color = "#ffd166", CreatedAt = now, UpdatedAt = now },
            new ClientTag { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "New", Color = "#8ecae6", CreatedAt = now, UpdatedAt = now },
            new ClientTag { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Returning", Color = "#90be6d", CreatedAt = now, UpdatedAt = now }
        };

        var clients = new[]
        {
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Anna", LastName = "Stone", Phone = "+1 555 0101", Email = "anna@example.com", TotalVisits = 2, Ltv = 220, CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Mark", LastName = "Lee", Phone = "+1 555 0102", Email = "mark@example.com", TotalVisits = 1, Ltv = 90, CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Sara", LastName = "Cole", Phone = "+1 555 0103", Email = "sara@example.com", TotalVisits = 3, Ltv = 340, CreatedAt = now, UpdatedAt = now }
        };

        db.Organizations.Add(organization);
        db.Branches.Add(branch);
        db.AspNetUsers.Add(user);
        db.Staff.Add(staff);
        db.ClientTags.AddRange(tags);
        db.Clients.AddRange(clients);

        db.ClientTagLinks.AddRange(
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[0].Id, ClientTagId = tags[0].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[0].Id, ClientTagId = tags[2].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[1].Id, ClientTagId = tags[1].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[2].Id, ClientTagId = tags[2].Id, CreatedAt = now });

        await db.SaveChangesAsync(cancellationToken);
    }
}
