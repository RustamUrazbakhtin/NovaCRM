using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class AnalyticsDailyStaff
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid StaffId { get; set; }

    public DateOnly MetricDate { get; set; }

    public int TotalAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public decimal RevenueTotal { get; set; }

    public decimal? HoursWorked { get; set; }

    public int NewClients { get; set; }

    public int ReviewsCount { get; set; }

    public decimal? AverageRating { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
