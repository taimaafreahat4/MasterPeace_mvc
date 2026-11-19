using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Review
{
    public long ReviewId { get; set; }

    public long OrderItemId { get; set; }

    public long ReviewerId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public string ApprovalStatus { get; set; } = null!;

    public long? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ModerationAction> ModerationActions { get; set; } = new List<ModerationAction>();

    public virtual OrderItem OrderItem { get; set; } = null!;

    public virtual User? ReviewedByUser { get; set; }

    public virtual User Reviewer { get; set; } = null!;
}
