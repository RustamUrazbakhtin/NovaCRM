using System;

namespace NovaCRM.Data.Model;

public partial class ClientSegmentDefinition
{
    public Guid Id { get; set; }

    public Guid? OrganizationId { get; set; }

    public string Key { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual Organization? Organization { get; set; }
}
