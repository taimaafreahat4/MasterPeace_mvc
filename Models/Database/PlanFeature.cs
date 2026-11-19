using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class PlanFeature
{
    public int FeatureId { get; set; }

    public string FeatureName { get; set; } = null!;

    public virtual ICollection<SubscriptionPlan> Plans { get; set; } = new List<SubscriptionPlan>();
}
