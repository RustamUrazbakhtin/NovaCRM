using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class AppointmentService
{
    public Guid AppointmentId { get; set; }

    public Guid ServiceId { get; set; }

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
