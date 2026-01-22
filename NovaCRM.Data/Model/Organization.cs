using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Organization
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Industry { get; set; }

    public string Timezone { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string PlanType { get; set; } = null!;

    public string SubscriptionStatus { get; set; } = null!;

    public string Settings { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AnalyticsDailyOrganization> AnalyticsDailyOrganizations { get; set; } = new List<AnalyticsDailyOrganization>();

    public virtual ICollection<AnalyticsDailyStaff> AnalyticsDailyStaffs { get; set; } = new List<AnalyticsDailyStaff>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<ClientNote> ClientNotes { get; set; } = new List<ClientNote>();

    public virtual ICollection<ClientMetric> ClientMetrics { get; set; } = new List<ClientMetric>();

    public virtual ICollection<ClientTag> ClientTags { get; set; } = new List<ClientTag>();

    public virtual ICollection<ClientTagLink> ClientTagLinks { get; set; } = new List<ClientTagLink>();

    public virtual ICollection<ClientSegmentDefinition> ClientSegmentDefinitions { get; set; } = new List<ClientSegmentDefinition>();

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<ExpenseCategory> ExpenseCategories { get; set; } = new List<ExpenseCategory>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<InventoryCategory> InventoryCategories { get; set; } = new List<InventoryCategory>();

    public virtual ICollection<InventoryItemStock> InventoryItemStocks { get; set; } = new List<InventoryItemStock>();

    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public virtual ICollection<InventoryLocation> InventoryLocations { get; set; } = new List<InventoryLocation>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<InvoiceSequence> InvoiceSequences { get; set; } = new List<InvoiceSequence>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<NotificationChannel> NotificationChannels { get; set; } = new List<NotificationChannel>();

    public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();

    public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();

    public virtual ICollection<StaffTimeOff> StaffTimeOffs { get; set; } = new List<StaffTimeOff>();

    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

    public virtual ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
