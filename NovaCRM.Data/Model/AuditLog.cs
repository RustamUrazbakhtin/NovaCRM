using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string? UserId { get; set; }

    public string EntityType { get; set; } = null!;

    public Guid? EntityId { get; set; }

    public string Action { get; set; } = null!;

    public string Metadata { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual AspNetUser? User { get; set; }
}
