using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class ServiceCategory
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
