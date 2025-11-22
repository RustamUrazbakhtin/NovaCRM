using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class InventoryItemStock
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ItemId { get; set; }

    public Guid LocationId { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual InventoryItem Item { get; set; } = null!;

    public virtual InventoryLocation Location { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
