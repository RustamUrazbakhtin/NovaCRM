using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaCRM.Server.Domain;

namespace NovaCRM.Server.Auth;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Staff> StaffMembers => Set<Staff>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");
            entity.Property(o => o.Name).IsRequired();
            entity.Property(o => o.Timezone).IsRequired();
            entity.Property(o => o.Country).IsRequired();
            entity.Property(o => o.BusinessId);
            entity.Property(o => o.Currency).HasDefaultValue("USD");
            entity.Property(o => o.PlanType).HasDefaultValue("free");
            entity.Property(o => o.SubscriptionStatus).HasDefaultValue("active");
        });

        builder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.Property(b => b.Name).IsRequired();
            entity.Property(b => b.IsDefault).HasDefaultValue(false);

            entity.HasOne(b => b.Organization)
                  .WithMany(o => o.Branches)
                  .HasForeignKey(b => b.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Staff>(entity =>
        {
            entity.ToTable("Staff");
            entity.Property(s => s.FirstName).IsRequired();
            entity.Property(s => s.LastName).IsRequired();
            entity.Property(s => s.IsActive).HasDefaultValue(true);

            entity.HasOne(s => s.Organization)
                  .WithMany(o => o.StaffMembers)
                  .HasForeignKey(s => s.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Branch)
                  .WithMany(b => b.StaffMembers)
                  .HasForeignKey(s => s.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
