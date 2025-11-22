using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class PurchaseOrder
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid SupplierId { get; set; }

    public Guid? BranchId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? OrderedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    public virtual Supplier Supplier { get; set; } = null!;
}
