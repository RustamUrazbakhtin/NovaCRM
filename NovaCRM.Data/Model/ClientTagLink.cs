using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ClientTagLink
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid ClientId { get; set; }

    public Guid ClientTagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual ClientTag Tag { get; set; } = null!;
}
