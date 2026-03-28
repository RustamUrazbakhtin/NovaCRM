using Microsoft.EntityFrameworkCore;
using NovaCRM.Data;
using NovaCRM.Data.Model;
using NovaCRM.Server.Services.Clients;

namespace NovaCRM.Server.Tests.Clients;

public class ClientRepositoryTests
{
    [Fact]
    public async Task Should_Load_Clients_With_Tags()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var orgId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using (var seedDbContext = new ApplicationDbContext(options))
        {
            seedDbContext.Organizations.Add(new Organization
            {
                Id = orgId,
                Name = "Test Org",
                Timezone = "UTC",
                CreatedAt = now,
                UpdatedAt = now
            });

            seedDbContext.Clients.Add(new Client
            {
                Id = clientId,
                OrganizationId = orgId,
                FirstName = "Ada",
                LastName = "Lovelace",
                Phone = "+10000000000",
                MarketingOptIn = false,
                TotalVisits = 1,
                Ltv = 125m,
                CreatedAt = now,
                UpdatedAt = now
            });

            seedDbContext.ClientTags.Add(new ClientTag
            {
                Id = tagId,
                OrganizationId = orgId,
                Name = "VIP",
                Color = "#ff6600",
                CreatedAt = now,
                UpdatedAt = now
            });

            seedDbContext.ClientTagLinks.Add(new ClientTagLink
            {
                ClientId = clientId,
                TagId = tagId,
                CreatedAt = now
            });

            await seedDbContext.SaveChangesAsync();
        }

        await using var queryDbContext = new ApplicationDbContext(options);
        var service = new ClientRepository(queryDbContext);

        var result = await service.GetClientsAsync(orgId, CancellationToken.None);

        Assert.NotNull(result);
        var client = Assert.Single(result);
        var tag = Assert.Single(client.Tags);
        Assert.Equal("VIP", tag.Name);
    }
}
