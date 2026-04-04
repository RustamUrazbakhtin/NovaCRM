using Microsoft.EntityFrameworkCore;
using NovaCRM.Data.Model;

namespace NovaCRM.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<Branch> Branches { get; set; }
    public virtual DbSet<Staff> Staff { get; set; }
    public virtual DbSet<Client> Clients { get; set; }
    public virtual DbSet<ClientTag> ClientTags { get; set; }
    public virtual DbSet<ClientTagLink> ClientTagLinks { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<StaffRole> StaffRoles { get; set; }
    public virtual DbSet<StaffRoleLink> StaffRoleLinks { get; set; }
    public virtual DbSet<StaffSpecialization> StaffSpecializations { get; set; }
    public virtual DbSet<StaffSpecializationLink> StaffSpecializationLinks { get; set; }
    public virtual DbSet<StaffCompensation> StaffCompensations { get; set; }
    public virtual DbSet<StaffCompensationHistory> StaffCompensationHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("EmailIndex");
            entity.HasIndex(e => e.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Timezone).IsRequired();
            entity.Property(e => e.Currency).HasDefaultValue("USD");
            entity.Property(e => e.Settings).HasDefaultValue("{}");
            entity.Property(e => e.PlanType).HasDefaultValue("free");
            entity.Property(e => e.SubscriptionStatus).HasDefaultValue("active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Organization).WithMany(p => p.Branches).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.EmploymentStatus).HasDefaultValue("Available");
            entity.Property(e => e.HasCrmAccess).HasDefaultValue(false);
            entity.Property(e => e.RatingAverage).HasPrecision(4, 2).HasDefaultValue(0m);
            entity.Property(e => e.RatingCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Organization).WithMany(p => p.Staff).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Branch).WithMany(p => p.Staff).HasForeignKey(d => d.BranchId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(d => d.User).WithMany(p => p.Staff).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.OrganizationId, e.IsActive, e.EmploymentStatus });
        });

        modelBuilder.Entity<StaffRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Code).IsRequired();
            entity.HasIndex(e => new { e.OrganizationId, e.Code }).IsUnique();
            entity.HasOne(d => d.Organization).WithMany(p => p.StaffRoles).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaffRoleLink>(entity =>
        {
            entity.HasKey(e => new { e.StaffId, e.RoleId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Staff).WithMany(p => p.StaffRoleLinks).HasForeignKey(d => d.StaffId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Role).WithMany(p => p.StaffRoleLinks).HasForeignKey(d => d.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaffSpecialization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => new { e.OrganizationId, e.Code }).IsUnique();
            entity.HasOne(d => d.Organization).WithMany(p => p.StaffSpecializations).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaffSpecializationLink>(entity =>
        {
            entity.HasKey(e => new { e.StaffId, e.SpecializationId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Staff).WithMany(p => p.StaffSpecializationLinks).HasForeignKey(d => d.StaffId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Specialization).WithMany(p => p.StaffSpecializationLinks).HasForeignKey(d => d.SpecializationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaffCompensation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompensationType).IsRequired();
            entity.Property(e => e.FixedSalary).HasPrecision(12, 2);
            entity.Property(e => e.HourlyRate).HasPrecision(12, 2);
            entity.Property(e => e.CommissionPercent).HasPrecision(5, 2);
            entity.Property(e => e.PerServiceAmount).HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Staff).WithMany(p => p.StaffCompensations).HasForeignKey(d => d.StaffId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.StaffId, e.EffectiveFrom });
        });

        modelBuilder.Entity<StaffCompensationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NewSnapshot).IsRequired();
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Staff).WithMany(p => p.StaffCompensationHistories).HasForeignKey(d => d.StaffId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.Ltv).HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Organization).WithMany(p => p.Clients).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Branch).WithMany(p => p.Clients).HasForeignKey(d => d.BranchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClientTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
            entity.HasOne(d => d.Organization).WithMany(p => p.ClientTags).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClientTagLink>(entity =>
        {
            entity.ToTable("ClientTagLinks");
            entity.HasKey(e => new { e.ClientId, e.TagId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Client).WithMany(p => p.ClientTagLinks).HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(d => d.Tag).WithMany(p => p.ClientTagLinks).HasForeignKey(d => d.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Price).HasPrecision(12, 2);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Organization).WithMany(p => p.Services).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Category).WithMany(p => p.Services).HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Source).HasDefaultValue("manual");
            entity.Property(e => e.PriceAtVisit).HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(d => d.Organization).WithMany(p => p.Appointments).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Branch).WithMany(p => p.Appointments).HasForeignKey(d => d.BranchId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(d => d.Client).WithMany(p => p.Appointments).HasForeignKey(d => d.ClientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Staff).WithMany(p => p.Appointments).HasForeignKey(d => d.StaffId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(d => d.Service).WithMany(p => p.Appointments).HasForeignKey(d => d.ServiceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.OrganizationId, e.StartAt });
        });

        var ownerRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111").ToString();
        var adminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222").ToString();

        modelBuilder.Entity<AspNetRole>().HasData(
            new AspNetRole { Id = ownerRoleId, Name = "Owner", NormalizedName = "OWNER", ConcurrencyStamp = ownerRoleId },
            new AspNetRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = adminRoleId });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
