using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class VwHomeFeedback
{
    public long FeedbackId { get; set; }

    public long? UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? Category { get; set; }

    public byte? Rating { get; set; }

    public string Message { get; set; } = null!;

    public DateTime SubmittedAt { get; set; }
}
