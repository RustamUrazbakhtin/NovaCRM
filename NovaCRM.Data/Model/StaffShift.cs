using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class StaffShift
{
    public Guid Id { get; set; }

    public Guid StaffId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid OrganizationId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
