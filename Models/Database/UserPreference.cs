using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class UserPreference
{
    public long UserId { get; set; }

    public bool OrderNotifications { get; set; }

    public bool ShippingNotifications { get; set; }

    public bool MarketingNotifications { get; set; }

    public virtual User User { get; set; } = null!;
}
