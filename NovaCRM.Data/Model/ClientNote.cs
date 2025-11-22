using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ClientNote
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public Guid OrganizationId { get; set; }

    public string? AuthorUserId { get; set; }

    public string Note { get; set; } = null!;

    public bool IsPrivate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AspNetUser? AuthorUser { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
