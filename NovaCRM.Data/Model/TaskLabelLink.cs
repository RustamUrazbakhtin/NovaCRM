using System;
using System.Collections.Generic;

namespace NovaCRM.Data.Model;

public partial class TaskLabelLink
{
    public Guid TaskId { get; set; }

    public Guid LabelId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TaskLabel Label { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
