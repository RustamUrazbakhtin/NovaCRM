using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class InventoryItem
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SupplierId { get; set; }

    public string Name { get; set; } = null!;

    public string? Sku { get; set; }

    public string Unit { get; set; } = null!;

    public int MinQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual InventoryCategory? Category { get; set; }

    public virtual ICollection<InventoryItemStock> InventoryItemStocks { get; set; } = new List<InventoryItemStock>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    public virtual Supplier? Supplier { get; set; }
}
