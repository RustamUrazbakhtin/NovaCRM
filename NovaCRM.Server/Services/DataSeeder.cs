using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;

namespace NovaCRM.Server.Services;

public static class DataSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
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
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Anna", LastName = "Stone", Phone = "+1 555 0101", Email = "anna@example.com", TotalVisits = 2, Ltv = 220, LastVisitAt = now.AddDays(-5), Notes = "Prefers balayage and warm tones.", CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Mark", LastName = "Lee", Phone = "+1 555 0102", Email = "mark@example.com", TotalVisits = 1, Ltv = 90, LastVisitAt = now.AddDays(-41), Notes = "Usually books beard trim and styling.", CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Sara", LastName = "Cole", Phone = "+1 555 0103", Email = "sara@example.com", TotalVisits = 3, Ltv = 340, LastVisitAt = now.AddDays(-12), Notes = "VIP treatment package customer.", CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Nina", LastName = "Wright", Phone = "+1 555 0104", Email = "nina@example.com", TotalVisits = 4, Ltv = 420, LastVisitAt = now.AddDays(-2), Notes = "Prefers morning slots and short appointments.", CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Olivia", LastName = "Grant", Phone = "+1 555 0105", Email = "olivia@example.com", TotalVisits = 1, Ltv = 115, LastVisitAt = now.AddDays(-64), Notes = "Follow up for no-show from previous booking.", CreatedAt = now, UpdatedAt = now },
            new Client { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, FirstName = "Elena", LastName = "Parker", Phone = "+1 555 0106", Email = "elena@example.com", TotalVisits = 6, Ltv = 690, LastVisitAt = now.AddDays(-9), Notes = "High LTV; usually books color + treatment bundle.", CreatedAt = now, UpdatedAt = now }
        };

        var services = new[]
        {
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Brows", DurationMinutes = 30, Price = 45, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Lashes", DurationMinutes = 75, Price = 120, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Manicure", DurationMinutes = 50, Price = 55, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Hair color", DurationMinutes = 120, Price = 180, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Facial", DurationMinutes = 60, Price = 95, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        var appointments = new[]
        {
            new Appointment { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, ClientId = clients[0].Id, ServiceId = services[3].Id, StaffId = staff.Id, StartAt = now.AddDays(-5).Date.AddHours(11), EndAt = now.AddDays(-5).Date.AddHours(13), Status = "Completed", Source = "seed", PriceAtVisit = services[3].Price, Notes = "Color refresh.", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, ClientId = clients[3].Id, ServiceId = services[1].Id, StaffId = staff.Id, StartAt = now.AddDays(2).Date.AddHours(10), EndAt = now.AddDays(2).Date.AddHours(11).AddMinutes(15), Status = "Scheduled", Source = "seed", PriceAtVisit = services[1].Price, Notes = "Patch test already completed.", CreatedAt = now, UpdatedAt = now }
        };

        db.Organizations.Add(organization);
        db.Branches.Add(branch);
        db.AspNetUsers.Add(user);
        db.Staff.Add(staff);
        db.ClientTags.AddRange(tags);
        db.Clients.AddRange(clients);
        db.Services.AddRange(services);
        db.Appointments.AddRange(appointments);

        db.ClientTagLinks.AddRange(
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[0].Id, ClientTagId = tags[0].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[0].Id, ClientTagId = tags[2].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[1].Id, ClientTagId = tags[1].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[2].Id, ClientTagId = tags[2].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[3].Id, ClientTagId = tags[0].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[4].Id, ClientTagId = tags[1].Id, CreatedAt = now },
            new ClientTagLink { Id = Guid.NewGuid(), OrganizationId = organization.Id, ClientId = clients[5].Id, ClientTagId = tags[2].Id, CreatedAt = now });

        await db.SaveChangesAsync(cancellationToken);
    }
}
