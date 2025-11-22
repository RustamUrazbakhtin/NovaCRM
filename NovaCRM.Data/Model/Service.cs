using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Service
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual ServiceCategory? Category { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<ServiceStaff> ServiceStaffs { get; set; } = new List<ServiceStaff>();
}
