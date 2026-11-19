using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string PlanCode { get; set; } = null!;

    public string PlanName { get; set; } = null!;

    public int DurationMonths { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
}
