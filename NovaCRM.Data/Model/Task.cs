using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Task
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public DateTime? DueAt { get; set; }

    public Guid? AssignedToStaffId { get; set; }

    public string CreatedByUserId { get; set; } = null!;

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Staff? AssignedToStaff { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual AspNetUser CreatedByUser { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

    public virtual ICollection<TaskLabelLink> TaskLabelLinks { get; set; } = new List<TaskLabelLink>();
}
