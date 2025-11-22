using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class InventoryMovement
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ItemId { get; set; }

    public Guid? LocationId { get; set; }

    public int QuantityChange { get; set; }

    public string Reason { get; set; } = null!;

    public Guid? RelatedAppointmentId { get; set; }

    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AspNetUser? CreatedByUser { get; set; }

    public virtual InventoryItem Item { get; set; } = null!;

    public virtual InventoryLocation? Location { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Appointment? RelatedAppointment { get; set; }
}
