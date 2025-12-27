using System;
using System.Collections.Generic;
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

    public virtual DbSet<AnalyticsDailyOrganization> AnalyticsDailyOrganizations { get; set; }

    public virtual DbSet<AnalyticsDailyStaff> AnalyticsDailyStaffs { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentService> AppointmentServices { get; set; }

    public virtual DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
    //public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientNote> ClientNotes { get; set; }

    public virtual DbSet<ClientSegmentDefinition> ClientSegmentDefinitions { get; set; }

    public virtual DbSet<ClientTag> ClientTags { get; set; }

    public virtual DbSet<ClientTagLink> ClientTagLinks { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseCategory> ExpenseCategories { get; set; }

    public virtual DbSet<InventoryCategory> InventoryCategories { get; set; }

    public virtual DbSet<InventoryItem> InventoryItems { get; set; }

    public virtual DbSet<InventoryItemStock> InventoryItemStocks { get; set; }

    public virtual DbSet<InventoryLocation> InventoryLocations { get; set; }

    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<InvoiceSequence> InvoiceSequences { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationChannel> NotificationChannels { get; set; }

    public virtual DbSet<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; }

    public virtual DbSet<NotificationPreference> NotificationPreferences { get; set; }

    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewReply> ReviewReplies { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<ServiceStaff> ServiceStaffs { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StaffShift> StaffShifts { get; set; }

    public virtual DbSet<StaffTimeOff> StaffTimeOffs { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Data.Model.Task> Tasks { get; set; }

    public virtual DbSet<TaskComment> TaskComments { get; set; }

    public virtual DbSet<TaskLabel> TaskLabels { get; set; }

    public virtual DbSet<TaskLabelLink> TaskLabelLinks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=NovaCRM_DB;Username=postgres;Password=NovaCRM_2025!StrongPass");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalyticsDailyOrganization>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_AnalyticsDailyOrganizations_BranchId");

            entity.HasIndex(e => e.OrganizationId, "IX_AnalyticsDailyOrganizations_OrganizationId");

            entity.HasIndex(e => new { e.OrganizationId, e.BranchId, e.MetricDate }, "IX_AnalyticsDailyOrganizations_Unique")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.CancelledAppointments).HasDefaultValue(0);
            entity.Property(e => e.CompletedAppointments).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ExpensesTotal).HasPrecision(12, 2);
            entity.Property(e => e.NewClients).HasDefaultValue(0);
            entity.Property(e => e.NoShowAppointments).HasDefaultValue(0);
            entity.Property(e => e.ReturningClients).HasDefaultValue(0);
            entity.Property(e => e.RevenueTotal).HasPrecision(12, 2);
            entity.Property(e => e.ReviewsCount).HasDefaultValue(0);
            entity.Property(e => e.TotalAppointments).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.AnalyticsDailyOrganizations)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.AnalyticsDailyOrganizations)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnalyticsDailyStaff>(entity =>
        {
            entity.ToTable("AnalyticsDailyStaff");

            entity.HasIndex(e => e.OrganizationId, "IX_AnalyticsDailyStaff_OrganizationId");

            entity.HasIndex(e => e.StaffId, "IX_AnalyticsDailyStaff_StaffId");

            entity.HasIndex(e => new { e.OrganizationId, e.StaffId, e.MetricDate }, "IX_AnalyticsDailyStaff_Unique")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.CompletedAppointments).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.HoursWorked).HasPrecision(6, 2);
            entity.Property(e => e.NewClients).HasDefaultValue(0);
            entity.Property(e => e.RevenueTotal).HasPrecision(12, 2);
            entity.Property(e => e.ReviewsCount).HasDefaultValue(0);
            entity.Property(e => e.TotalAppointments).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.AnalyticsDailyStaffs)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Staff).WithMany(p => p.AnalyticsDailyStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_Appointments_BranchId");

            entity.HasIndex(e => e.ClientId, "IX_Appointments_ClientId");

            entity.HasIndex(e => new { e.ClientId, e.StartAt }, "IX_Appointments_ClientId_StartAt");

            entity.HasIndex(e => e.OrganizationId, "IX_Appointments_OrganizationId");

            entity.HasIndex(e => new { e.StaffId, e.StartAt }, "IX_Appointments_StaffId_StartAt");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Source).HasDefaultValueSql("'manual'::text");
            entity.Property(e => e.Status).HasDefaultValueSql("'scheduled'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Staff).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.HasKey(e => new { e.AppointmentId, e.ServiceId });

            entity.HasIndex(e => e.ServiceId, "IX_AppointmentServices_ServiceId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            entity.Property(e => e.Price).HasPrecision(12, 2);

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentServices).HasForeignKey(d => d.AppointmentId);

            entity.HasOne(d => d.Service).WithMany(p => p.AppointmentServices).HasForeignKey(d => d.ServiceId);
        });

        modelBuilder.Entity<AppointmentStatusHistory>(entity =>
        {
            entity.ToTable("AppointmentStatusHistory");

            entity.HasIndex(e => e.AppointmentId, "IX_AppointmentStatusHistory_AppointmentId");

            entity.HasIndex(e => e.ChangedByUserId, "IX_AppointmentStatusHistory_ChangedByUserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentStatusHistories).HasForeignKey(d => d.AppointmentId);

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.AppointmentStatusHistories)
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_AuditLogs_OrganizationId");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Metadata)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_Branches_OrganizationId");

            entity.HasIndex(e => e.OrganizationId, "IX_Branches_OrganizationId_IsDefault")
                .IsUnique()
                .HasFilter("((\"IsDefault\" = true) AND (\"DeletedAt\" IS NULL))");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithOne(p => p.Branch)
                .HasForeignKey<Branch>(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_Clients_BranchId");

            entity.HasIndex(e => e.Email, "IX_Clients_Email");

            entity.HasIndex(e => e.OrganizationId, "IX_Clients_OrganizationId");

            entity.HasIndex(e => e.Phone, "IX_Clients_Phone");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Ltv).HasPrecision(12, 2);
            entity.Property(e => e.MarketingOptIn).HasDefaultValue(true);
            entity.Property(e => e.TotalVisits).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.Clients)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Clients)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClientSegmentDefinition>(entity =>
        {
            entity.HasIndex(e => new { e.OrganizationId, e.Key }, "IX_ClientSegmentDefinitions_OrganizationId_Key")
                .IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Key).HasMaxLength(64);
            entity.Property(e => e.Label).HasMaxLength(128);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Organization).WithMany(p => p.ClientSegmentDefinitions)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new ClientSegmentDefinition
                {
                    Id = Guid.Parse("3f7f0a73-b38b-4e9f-94d1-0d0f2b5759d1"),
                    OrganizationId = null,
                    Key = "All",
                    Label = "All",
                    SortOrder = 1,
                    IsActive = true
                },
                new ClientSegmentDefinition
                {
                    Id = Guid.Parse("2aaf7d9a-487d-4b33-9b30-cc6ad3a2e6d1"),
                    OrganizationId = null,
                    Key = "Vip",
                    Label = "VIP",
                    SortOrder = 2,
                    IsActive = true
                },
                new ClientSegmentDefinition
                {
                    Id = Guid.Parse("8f2b6d4d-1b46-4e9c-9f7a-e0b0a1e4e0e5"),
                    OrganizationId = null,
                    Key = "Regular",
                    Label = "Regular",
                    SortOrder = 3,
                    IsActive = true
                },
                new ClientSegmentDefinition
                {
                    Id = Guid.Parse("5a3b917a-e09f-4e58-90c1-5d8d4bfa3fb9"),
                    OrganizationId = null,
                    Key = "New",
                    Label = "New",
                    SortOrder = 4,
                    IsActive = true
                },
                new ClientSegmentDefinition
                {
                    Id = Guid.Parse("6dfc2ea0-62e9-4316-9d59-8285343334d2"),
                    OrganizationId = null,
                    Key = "AtRisk",
                    Label = "At risk",
                    SortOrder = 5,
                    IsActive = true
                }
            );
        });

        modelBuilder.Entity<ClientNote>(entity =>
        {
            entity.HasIndex(e => e.AuthorUserId, "IX_ClientNotes_AuthorUserId");

            entity.HasIndex(e => e.ClientId, "IX_ClientNotes_ClientId");

            entity.HasIndex(e => e.OrganizationId, "IX_ClientNotes_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsPrivate).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.ClientNotes)
                .HasForeignKey(d => d.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client).WithMany(p => p.ClientNotes)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.ClientNotes)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClientTag>(entity =>
        {
            entity.HasIndex(e => new { e.OrganizationId, e.Name }, "IX_ClientTags_OrganizationId_Name")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.ClientTags)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClientTagLink>(entity =>
        {
            entity.HasKey(e => new { e.ClientId, e.TagId });

            entity.HasIndex(e => e.TagId, "IX_ClientTagLinks_TagId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Client).WithMany(p => p.ClientTagLinks).HasForeignKey(d => d.ClientId);

            entity.HasOne(d => d.Tag).WithMany(p => p.ClientTagLinks).HasForeignKey(d => d.TagId);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_Expenses_BranchId");

            entity.HasIndex(e => e.CategoryId, "IX_Expenses_CategoryId");

            entity.HasIndex(e => e.OrganizationId, "IX_Expenses_OrganizationId");

            entity.HasIndex(e => e.SupplierId, "IX_Expenses_SupplierId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Category).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_ExpenseCategories_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.ExpenseCategories)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryCategory>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_InventoryCategories_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.InventoryCategories)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_InventoryItems_CategoryId");

            entity.HasIndex(e => e.OrganizationId, "IX_InventoryItems_OrganizationId");

            entity.HasIndex(e => e.SupplierId, "IX_InventoryItems_SupplierId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.MinQuantity).HasDefaultValue(0);
            entity.Property(e => e.Unit).HasDefaultValueSql("'pcs'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Category).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Supplier).WithMany(p => p.InventoryItems)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryItemStock>(entity =>
        {
            entity.HasIndex(e => e.ItemId, "IX_InventoryItemStocks_ItemId");

            entity.HasIndex(e => new { e.ItemId, e.LocationId }, "IX_InventoryItemStocks_Item_Location")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.HasIndex(e => e.LocationId, "IX_InventoryItemStocks_LocationId");

            entity.HasIndex(e => e.OrganizationId, "IX_InventoryItemStocks_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryItemStocks)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Location).WithMany(p => p.InventoryItemStocks)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.InventoryItemStocks)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryLocation>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_InventoryLocations_BranchId");

            entity.HasIndex(e => e.OrganizationId, "IX_InventoryLocations_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.InventoryLocations)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.InventoryLocations)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasIndex(e => e.CreatedByUserId, "IX_InventoryMovements_CreatedByUserId");

            entity.HasIndex(e => e.ItemId, "IX_InventoryMovements_ItemId");

            entity.HasIndex(e => e.LocationId, "IX_InventoryMovements_LocationId");

            entity.HasIndex(e => e.OrganizationId, "IX_InventoryMovements_OrganizationId");

            entity.HasIndex(e => e.RelatedAppointmentId, "IX_InventoryMovements_RelatedAppointmentId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Item).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Location).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.RelatedAppointment).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.RelatedAppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_Invoices_BranchId");

            entity.HasIndex(e => e.ClientId, "IX_Invoices_ClientId");

            entity.HasIndex(e => new { e.OrganizationId, e.Number }, "IX_Invoices_Number_Unique")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.HasIndex(e => e.OrganizationId, "IX_Invoices_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'draft'::text");
            entity.Property(e => e.Subtotal).HasPrecision(12, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(12, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasIndex(e => e.InvoiceId, "IX_InvoiceItems_InvoiceId");

            entity.HasIndex(e => e.ServiceId, "IX_InvoiceItems_ServiceId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LineTotal).HasPrecision(12, 2);
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("1.0");
            entity.Property(e => e.UnitPrice).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Service).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceSequence>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_InvoiceSequences_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LastNumber).HasDefaultValue(0);
            entity.Property(e => e.Prefix).HasDefaultValueSql("'INV-'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.InvoiceSequences)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.ChannelId, "IX_Notifications_ChannelId");

            entity.HasIndex(e => e.OrganizationId, "IX_Notifications_OrganizationId");

            entity.HasIndex(e => e.RecipientClientId, "IX_Notifications_RecipientClientId");

            entity.HasIndex(e => e.RecipientUserId, "IX_Notifications_RecipientUserId");

            entity.HasIndex(e => e.TemplateId, "IX_Notifications_TemplateId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Payload)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Channel).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.RecipientClient).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RecipientClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.RecipientUser).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Template).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationChannel>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_NotificationChannels_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Config)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.NotificationChannels)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationDeliveryLog>(entity =>
        {
            entity.HasIndex(e => e.NotificationId, "IX_NotificationDeliveryLogs_NotificationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationDeliveryLogs).HasForeignKey(d => d.NotificationId);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasIndex(e => e.ChannelId, "IX_NotificationPreferences_ChannelId");

            entity.HasIndex(e => e.ClientId, "IX_NotificationPreferences_ClientId");

            entity.HasIndex(e => e.OrganizationId, "IX_NotificationPreferences_OrganizationId");

            entity.HasIndex(e => e.UserId, "IX_NotificationPreferences_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.Settings)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Channel).WithMany(p => p.NotificationPreferences)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client).WithMany(p => p.NotificationPreferences)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.NotificationPreferences)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User).WithMany(p => p.NotificationPreferences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasIndex(e => e.ChannelId, "IX_NotificationTemplates_ChannelId");

            entity.HasIndex(e => e.OrganizationId, "IX_NotificationTemplates_OrganizationId");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }, "IX_NotificationTemplates_Unique")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Channel).WithMany(p => p.NotificationTemplates)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.NotificationTemplates)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_Organizations_Name");

            entity.HasIndex(e => e.Name, "IX_Organizations_Name_Unique")
                .IsUnique()
                .HasFilter("(\"DeletedAt\" IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Currency).HasDefaultValueSql("'USD'::text");
            entity.Property(e => e.PlanType).HasDefaultValueSql("'free'::text");
            entity.Property(e => e.Settings)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.SubscriptionStatus).HasDefaultValueSql("'active'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.AppointmentId, "IX_Payments_AppointmentId");

            entity.HasIndex(e => e.BranchId, "IX_Payments_BranchId");

            entity.HasIndex(e => e.ClientId, "IX_Payments_ClientId");

            entity.HasIndex(e => e.InvoiceId, "IX_Payments_InvoiceId");

            entity.HasIndex(e => e.OrganizationId, "IX_Payments_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'paid'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Branch).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_PurchaseOrders_BranchId");

            entity.HasIndex(e => e.OrganizationId, "IX_PurchaseOrders_OrganizationId");

            entity.HasIndex(e => e.SupplierId, "IX_PurchaseOrders_SupplierId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'draft'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasIndex(e => e.ItemId, "IX_PurchaseOrderItems_ItemId");

            entity.HasIndex(e => e.PurchaseOrderId, "IX_PurchaseOrderItems_PurchaseOrderId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UnitPrice).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Item).WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(e => e.AppointmentId, "IX_Reviews_AppointmentId");

            entity.HasIndex(e => e.BranchId, "IX_Reviews_BranchId");

            entity.HasIndex(e => e.ClientId, "IX_Reviews_ClientId");

            entity.HasIndex(e => e.OrganizationId, "IX_Reviews_OrganizationId");

            entity.HasIndex(e => e.StaffId, "IX_Reviews_StaffId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Source).HasDefaultValueSql("'internal'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Branch).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Staff).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReviewReply>(entity =>
        {
            entity.HasIndex(e => e.AuthorUserId, "IX_ReviewReplies_AuthorUserId");

            entity.HasIndex(e => e.ReviewId, "IX_ReviewReplies_ReviewId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.ReviewReplies)
                .HasForeignKey(d => d.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewReplies)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Services_CategoryId");

            entity.HasIndex(e => e.OrganizationId, "IX_Services_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasPrecision(12, 2);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Services)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_ServiceCategories_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.ServiceCategories)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceStaff>(entity =>
        {
            entity.HasKey(e => new { e.ServiceId, e.StaffId });

            entity.ToTable("ServiceStaff");

            entity.HasIndex(e => e.StaffId, "IX_ServiceStaff_StaffId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceStaffs).HasForeignKey(d => d.ServiceId);

            entity.HasOne(d => d.Staff).WithMany(p => p.ServiceStaffs).HasForeignKey(d => d.StaffId);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_Staff_BranchId");

            entity.HasIndex(e => e.OrganizationId, "IX_Staff_OrganizationId");

            entity.HasIndex(e => e.UserId, "IX_Staff_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Rating).HasPrecision(2, 1);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.Staff)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Staff)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User).WithMany(p => p.Staff)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StaffShift>(entity =>
        {
            entity.HasIndex(e => e.BranchId, "IX_StaffShifts_BranchId");

            entity.HasIndex(e => e.OrganizationId, "IX_StaffShifts_OrganizationId");

            entity.HasIndex(e => e.StaffId, "IX_StaffShifts_StaffId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'scheduled'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Branch).WithMany(p => p.StaffShifts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.StaffShifts)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffShifts)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StaffTimeOff>(entity =>
        {
            entity.ToTable("StaffTimeOff");

            entity.HasIndex(e => e.ApprovedByUserId, "IX_StaffTimeOff_ApprovedByUserId");

            entity.HasIndex(e => e.OrganizationId, "IX_StaffTimeOff_OrganizationId");

            entity.HasIndex(e => e.StaffId, "IX_StaffTimeOff_StaffId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.StaffTimeOffs)
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.StaffTimeOffs)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffTimeOffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_Suppliers_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Data.Model.Task>(entity =>
        {
            entity.HasIndex(e => e.AssignedToStaffId, "IX_Tasks_AssignedToStaffId");

            entity.HasIndex(e => e.BranchId, "IX_Tasks_BranchId");

            entity.HasIndex(e => e.CreatedByUserId, "IX_Tasks_CreatedByUserId");

            entity.HasIndex(e => e.OrganizationId, "IX_Tasks_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Priority).HasDefaultValueSql("'medium'::text");
            entity.Property(e => e.Status).HasDefaultValueSql("'open'::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AssignedToStaff).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssignedToStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Branch).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organization).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasIndex(e => e.AuthorUserId, "IX_TaskComments_AuthorUserId");

            entity.HasIndex(e => e.TaskId, "IX_TaskComments_TaskId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Task).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskLabel>(entity =>
        {
            entity.HasIndex(e => e.OrganizationId, "IX_TaskLabels_OrganizationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Organization).WithMany(p => p.TaskLabels)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskLabelLink>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.LabelId });

            entity.HasIndex(e => e.LabelId, "IX_TaskLabelLinks_LabelId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Label).WithMany(p => p.TaskLabelLinks).HasForeignKey(d => d.LabelId);

            entity.HasOne(d => d.Task).WithMany(p => p.TaskLabelLinks).HasForeignKey(d => d.TaskId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
