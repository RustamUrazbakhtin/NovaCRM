using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Staff
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public string? UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? RoleTitle { get; set; }

    public decimal? Rating { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; }
    public string? AvatarUrl { get; set; }

    public DateOnly? HiredAt { get; set; }

    public DateTime? Birthday { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AnalyticsDailyStaff> AnalyticsDailyStaffs { get; set; } = new List<AnalyticsDailyStaff>();

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Branch? Branch { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<ServiceStaff> ServiceStaffs { get; set; } = new List<ServiceStaff>();

    public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();

    public virtual ICollection<StaffTimeOff> StaffTimeOffs { get; set; } = new List<StaffTimeOff>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual AspNetUser? User { get; set; }
}
