using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class NotificationTemplate
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public Guid ChannelId { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual NotificationChannel Channel { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Organization Organization { get; set; } = null!;
}
