using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Expense
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SupplierId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public DateOnly ExpenseDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ExpenseCategory? Category { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Supplier? Supplier { get; set; }
}
