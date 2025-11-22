using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Appointment
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid ClientId { get; set; }

    public Guid StaffId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string Status { get; set; } = null!;

    public string Source { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual ICollection<AppointmentStatusHistory> AppointmentStatusHistories { get; set; } = new List<AppointmentStatusHistory>();

    public virtual Branch? Branch { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Staff Staff { get; set; } = null!;
}
