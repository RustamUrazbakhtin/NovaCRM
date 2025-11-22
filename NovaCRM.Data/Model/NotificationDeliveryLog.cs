using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class NotificationDeliveryLog
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public string Status { get; set; } = null!;

    public string? ProviderResponse { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;
}
