using System;

namespace NovaCRM.Data.Model;

public partial class ClientMetric
{
    public Guid ClientId { get; set; }

    public Guid OrganizationId { get; set; }

    public DateTime? LastVisitAt { get; set; }

    public decimal? LifetimeValue { get; set; }

    public string? Status { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
