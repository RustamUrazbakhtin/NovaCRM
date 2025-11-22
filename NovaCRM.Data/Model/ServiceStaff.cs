using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ServiceStaff
{
    public Guid ServiceId { get; set; }

    public Guid StaffId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
