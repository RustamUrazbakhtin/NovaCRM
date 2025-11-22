using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class NotificationPreference
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string? UserId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid ChannelId { get; set; }

    public bool IsEnabled { get; set; }

    public string Settings { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual NotificationChannel Channel { get; set; } = null!;

    public virtual Client? Client { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual AspNetUser? User { get; set; }
}
