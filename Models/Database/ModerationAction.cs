using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class ModerationAction
{
    public long ActionId { get; set; }

    public long ModeratorUserId { get; set; }

    public long? ProductId { get; set; }

    public long? ReviewId { get; set; }

    public long? FeedbackId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SiteFeedback? Feedback { get; set; }

    public virtual User ModeratorUser { get; set; } = null!;

    public virtual Product? Product { get; set; }

    public virtual Review? Review { get; set; }
}
