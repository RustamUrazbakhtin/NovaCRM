using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ClientTagLink
{
    public Guid ClientId { get; set; }

    public Guid TagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ClientTag Tag { get; set; } = null!;
}
