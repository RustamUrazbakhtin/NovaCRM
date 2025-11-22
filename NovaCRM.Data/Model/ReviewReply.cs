using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ReviewReply
{
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }

    public string AuthorUserId { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AspNetUser AuthorUser { get; set; } = null!;

    public virtual Review Review { get; set; } = null!;
}
