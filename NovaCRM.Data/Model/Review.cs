using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class Review
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? StaffId { get; set; }

    public Guid? AppointmentId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string Source { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<ReviewReply> ReviewReplies { get; set; } = new List<ReviewReply>();

    public virtual Staff? Staff { get; set; }
}
