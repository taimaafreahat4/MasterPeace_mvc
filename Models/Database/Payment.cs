using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Payment
{
    public long PaymentId { get; set; }

    public long? OrderId { get; set; }

    public long? SubscriptionId { get; set; }

    public long? UserId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? ProviderReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Subscription? Subscription { get; set; }

    public virtual User? User { get; set; }
}
