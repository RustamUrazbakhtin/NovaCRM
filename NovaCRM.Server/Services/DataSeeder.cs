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

        if (await db.AspNetUsers.AnyAsync(cancellationToken)) return;

        var now = DateTime.UtcNow;
        var organization = new Organization { Id = Guid.NewGuid(), Name = "NovaCRM Demo", Timezone = "UTC", Currency = "USD", PlanType = "free", SubscriptionStatus = "active", Phone = "+1 555 0100", Email = "owner@novacrm.demo", Settings = "{}", CreatedAt = now, UpdatedAt = now };
        var downtown = new Branch { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Downtown Studio", City = "New York", Country = "US", Timezone = "UTC", IsDefault = true, CreatedAt = now, UpdatedAt = now };
        var uptown = new Branch { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Uptown Lounge", City = "New York", Country = "US", Timezone = "UTC", IsDefault = false, CreatedAt = now, UpdatedAt = now };

        var user = new AspNetUser { Id = Guid.NewGuid().ToString(), UserName = "owner@novacrm.demo", NormalizedUserName = "OWNER@NOVACRM.DEMO", Email = "owner@novacrm.demo", NormalizedEmail = "OWNER@NOVACRM.DEMO", EmailConfirmed = true, SecurityStamp = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString(), LockoutEnabled = true, AccessFailedCount = 0 };
        user.PasswordHash = passwordHasher.HashPassword(user, "NovaCRM123!");

        var ownerRole = await db.AspNetRoles.FirstAsync(r => r.NormalizedName == "OWNER", cancellationToken);
        var adminRole = await db.AspNetRoles.FirstAsync(r => r.NormalizedName == "ADMIN", cancellationToken);
        user.Roles.Add(ownerRole);
        user.Roles.Add(adminRole);

        var roles = new[]
        {
            new StaffRole { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Owner", Code = "owner", SortOrder = 1, IsSystem = true },
            new StaffRole { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Admin", Code = "admin", SortOrder = 2, IsSystem = true },
            new StaffRole { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Specialist", Code = "specialist", SortOrder = 3, IsSystem = true },
            new StaffRole { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Manager", Code = "manager", SortOrder = 4, IsSystem = true }
        };

        var specializationsData = new (string Name, string Code, string Category)[]
        {
            ("Lashes", "lashes", "Eyes"), ("Brows", "brows", "Eyes"), ("Nail Technician", "nail-tech", "Nails"), ("Manicure", "manicure", "Nails"),
            ("Pedicure", "pedicure", "Nails"), ("Hair Stylist", "hair-stylist", "Hair"), ("Barber", "barber", "Hair"), ("Colorist", "colorist", "Hair"),
            ("Wax Specialist", "wax", "Body"), ("Laser Technician", "laser", "Body"), ("Esthetician", "esthetician", "Skin"), ("Cosmetologist", "cosmetologist", "Skin"),
            ("Injector / Botox", "injector", "Medical"), ("Massage Therapist", "massage", "Wellness"), ("Front Desk / Reception", "front-desk", "Operations"),
            ("Admin Operations", "admin-ops", "Operations"), ("Manager", "manager-spec", "Operations"), ("Owner", "owner-spec", "Operations")
        };
        var specializations = specializationsData.Select((x, i) => new StaffSpecialization { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = x.Name, Code = x.Code, Category = x.Category, SortOrder = i + 1, IsActive = true }).ToArray();

        Staff staff(string first, string last, Branch branch, string status, decimal rating, int count, bool hasCrmAccess, string? userId = null) => new()
        {
            Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = branch.Id, HasCrmAccess = hasCrmAccess, UserId = hasCrmAccess ? userId : null, FirstName = first, LastName = last, RoleTitle = "Team", Phone = $"+1 555 01{Random.Shared.Next(10,99)}", Email = $"{first.ToLower()}.{last.ToLower()}@novacrm.demo", IsActive = true, EmploymentStatus = status, RatingAverage = rating, RatingCount = count, CreatedAt = now, UpdatedAt = now
        };

        var staffMembers = new[]
        {
            staff("Demo", "Owner", downtown, "Active", 4.9m, 55, true, user.Id),
            staff("Maya", "Lash", downtown, "Active", 4.8m, 31, false),
            staff("Olga", "Admin", downtown, "Active", 4.7m, 24, true),
            staff("Iris", "Inject", uptown, "Active", 4.95m, 40, true),
            staff("Nora", "Nails", uptown, "Active", 4.6m, 22, false),
            staff("Helen", "Hair", downtown, "OnLeave", 4.5m, 18, false),
            staff("Sam", "Manager", downtown, "Terminated", 4.4m, 11, true)
        };

        var roleMap = roles.ToDictionary(x => x.Code);
        var specMap = specializations.ToDictionary(x => x.Code);

        var roleLinks = new[]
        {
            (staffMembers[0], new[]{"owner","admin","specialist"}),
            (staffMembers[1], new[]{"specialist"}),
            (staffMembers[2], new[]{"admin","specialist"}),
            (staffMembers[3], new[]{"owner","specialist"}),
            (staffMembers[4], new[]{"specialist"}),
            (staffMembers[5], new[]{"specialist"}),
            (staffMembers[6], new[]{"manager","admin"})
        }.SelectMany(x => x.Item2.Select(code => new StaffRoleLink { StaffId = x.Item1.Id, RoleId = roleMap[code].Id, CreatedAt = now })).ToArray();

        var specLinks = new[]
        {
            (staffMembers[0], new[]{"lashes","injector","laser"}),
            (staffMembers[1], new[]{"lashes","brows"}),
            (staffMembers[2], new[]{"massage","wax","laser"}),
            (staffMembers[3], new[]{"injector","laser","lashes"}),
            (staffMembers[4], new[]{"nail-tech","manicure","pedicure"}),
            (staffMembers[5], new[]{"hair-stylist","colorist"}),
            (staffMembers[6], new[]{"manager-spec","admin-ops"})
        }.SelectMany(x => x.Item2.Where(specMap.ContainsKey).Select(code => new StaffSpecializationLink { StaffId = x.Item1.Id, SpecializationId = specMap[code].Id, CreatedAt = now })).ToArray();

        var compensations = new[]
        {
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[0].Id,CompensationType="Hybrid",FixedSalary=6000,CommissionPercent=12,EffectiveFrom=now.AddMonths(-2),Notes="Owner compensation",CreatedAt=now},
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[1].Id,CompensationType="Commission",CommissionPercent=35,PerServiceAmount=15,EffectiveFrom=now.AddMonths(-1),CreatedAt=now},
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[2].Id,CompensationType="Hourly",HourlyRate=35,EffectiveFrom=now.AddMonths(-3),CreatedAt=now},
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[3].Id,CompensationType="Hybrid",FixedSalary=4500,CommissionPercent=20,EffectiveFrom=now.AddMonths(-1),CreatedAt=now},
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[4].Id,CompensationType="Fixed",FixedSalary=3800,EffectiveFrom=now.AddMonths(-1),CreatedAt=now},
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[5].Id,CompensationType="Fixed",FixedSalary=4200,EffectiveFrom=now.AddMonths(-1),CreatedAt=now},
            new StaffCompensation{Id=Guid.NewGuid(),StaffId=staffMembers[6].Id,CompensationType="Fixed",FixedSalary=5000,EffectiveFrom=now.AddMonths(-1),CreatedAt=now}
        };

        var services = new[]
        {
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Brows", DurationMinutes = 30, Price = 45, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Lashes", DurationMinutes = 75, Price = 120, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Manicure", DurationMinutes = 50, Price = 55, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Hair color", DurationMinutes = 120, Price = 180, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Service { Id = Guid.NewGuid(), OrganizationId = organization.Id, Name = "Massage", DurationMinutes = 60, Price = 95, IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        var clients = Enumerable.Range(1, 6).Select(i => new Client
        {
            Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = i % 2 == 0 ? uptown.Id : downtown.Id, FirstName = $"Client{i}", LastName = "Demo", Phone = $"+1 555 010{i}", Email = $"client{i}@example.com", TotalVisits = i, Ltv = 100 + i * 90, LastVisitAt = now.AddDays(-i), CreatedAt = now, UpdatedAt = now
        }).ToArray();

        var appointments = new List<Appointment>();
        for (var i = 0; i < staffMembers.Length; i++)
        {
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = staffMembers[i].BranchId, ClientId = clients[i % clients.Length].Id, ServiceId = services[i % services.Length].Id,
                StaffId = staffMembers[i].Id, StartAt = now.Date.AddHours(10 + i), EndAt = now.Date.AddHours(11 + i), Status = i % 3 == 0 ? "InService" : "Scheduled", Source = "seed", PriceAtVisit = services[i % services.Length].Price, CreatedAt = now, UpdatedAt = now
            });
            appointments.Add(new Appointment
            {
                Id = Guid.NewGuid(), OrganizationId = organization.Id, BranchId = staffMembers[i].BranchId, ClientId = clients[(i+1) % clients.Length].Id, ServiceId = services[(i+1) % services.Length].Id,
                StaffId = staffMembers[i].Id, StartAt = now.Date.AddDays(2).AddHours(11 + i), EndAt = now.Date.AddDays(2).AddHours(12 + i), Status = "Scheduled", Source = "seed", PriceAtVisit = services[(i+1) % services.Length].Price, CreatedAt = now, UpdatedAt = now
            });
        }

        db.Organizations.Add(organization);
        db.Branches.AddRange(downtown, uptown);
        db.AspNetUsers.Add(user);
        db.StaffRoles.AddRange(roles);
        db.StaffSpecializations.AddRange(specializations);
        db.Staff.AddRange(staffMembers);
        db.StaffRoleLinks.AddRange(roleLinks);
        db.StaffSpecializationLinks.AddRange(specLinks);
        db.StaffCompensations.AddRange(compensations);
        db.StaffCompensationHistories.AddRange(compensations.Select(c => new StaffCompensationHistory{Id=Guid.NewGuid(),StaffId=c.StaffId,NewSnapshot=$"{c.CompensationType}:{c.FixedSalary}:{c.HourlyRate}:{c.CommissionPercent}:{c.PerServiceAmount}",ChangedAt=now,Notes="Initial seed compensation"}));
        db.Clients.AddRange(clients);
        db.Services.AddRange(services);
        db.Appointments.AddRange(appointments);

        await db.SaveChangesAsync(cancellationToken);
    }
}
