using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Client
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public DateOnly? Birthday { get; set; }

    public string? Gender { get; set; }

    public string? Segment { get; set; }

    public string? Notes { get; set; }

    public bool MarketingOptIn { get; set; }

    public DateTime? LastVisitAt { get; set; }

    public int TotalVisits { get; set; }

    public decimal Ltv { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Branch? Branch { get; set; }

    public virtual ClientMetric? ClientMetric { get; set; }

    public virtual ICollection<ClientNote> ClientNotes { get; set; } = new List<ClientNote>();

    public virtual ICollection<ClientTagLink> ClientTagLinks { get; set; } = new List<ClientTagLink>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
