using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class AuditLog
{
    public long AuditId { get; set; }

    public long? ActorUserId { get; set; }

    public string ActionName { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? DetailsJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }
}
