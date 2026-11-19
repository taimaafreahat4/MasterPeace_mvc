using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class SiteFeedback
{
    public long FeedbackId { get; set; }

    public long? UserId { get; set; }

    public string SourceType { get; set; } = null!;

    public long? OrderId { get; set; }

    public string? DisplayName { get; set; }

    public string? ContactEmail { get; set; }

    public string? Category { get; set; }

    public byte? Rating { get; set; }

    public string Message { get; set; } = null!;

    public string ApprovalStatus { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }

    public long? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public virtual ICollection<ModerationAction> ModerationActions { get; set; } = new List<ModerationAction>();

    public virtual Order? Order { get; set; }

    public virtual User? ReviewedByUser { get; set; }

    public virtual User? User { get; set; }
}
