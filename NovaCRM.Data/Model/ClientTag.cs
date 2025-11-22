using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ClientTag
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ClientTagLink> ClientTagLinks { get; set; } = new List<ClientTagLink>();

    public virtual Organization Organization { get; set; } = null!;
}
