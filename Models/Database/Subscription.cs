using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Subscription
{
    public long SubscriptionId { get; set; }

    public long SellerId { get; set; }

    public int PlanId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public decimal PricePaid { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual SellerProfile Seller { get; set; } = null!;
}
