using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? TemplateId { get; set; }

    public Guid ChannelId { get; set; }

    public string? RecipientUserId { get; set; }

    public Guid? RecipientClientId { get; set; }

    public string? RecipientAddress { get; set; }

    public string Payload { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual NotificationChannel Channel { get; set; } = null!;

    public virtual ICollection<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; } = new List<NotificationDeliveryLog>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual Client? RecipientClient { get; set; }

    public virtual AspNetUser? RecipientUser { get; set; }

    public virtual NotificationTemplate? Template { get; set; }
}
