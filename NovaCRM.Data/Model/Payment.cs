using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Method { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
