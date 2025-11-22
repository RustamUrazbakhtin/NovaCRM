using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class InvoiceSequence
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Prefix { get; set; } = null!;

    public int LastNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
