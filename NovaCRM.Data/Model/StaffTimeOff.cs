using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class StaffTimeOff
{
    public Guid Id { get; set; }

    public Guid StaffId { get; set; }

    public Guid OrganizationId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Reason { get; set; }

    public string? ApprovedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AspNetUser? ApprovedByUser { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
