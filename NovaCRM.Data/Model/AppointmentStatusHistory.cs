using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class AppointmentStatusHistory
{
    public Guid Id { get; set; }

    public Guid AppointmentId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public string? ChangedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual AspNetUser? ChangedByUser { get; set; }
}
