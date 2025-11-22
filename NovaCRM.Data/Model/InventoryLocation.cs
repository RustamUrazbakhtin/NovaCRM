using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class InventoryLocation
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<InventoryItemStock> InventoryItemStocks { get; set; } = new List<InventoryItemStock>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Organization Organization { get; set; } = null!;
}
