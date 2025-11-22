using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class AnalyticsDailyOrganization
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid? BranchId { get; set; }

    public DateOnly MetricDate { get; set; }

    public int TotalAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int CancelledAppointments { get; set; }

    public int NoShowAppointments { get; set; }

    public int NewClients { get; set; }

    public int ReturningClients { get; set; }

    public decimal RevenueTotal { get; set; }

    public decimal ExpensesTotal { get; set; }

    public int ReviewsCount { get; set; }

    public decimal? AverageRating { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
