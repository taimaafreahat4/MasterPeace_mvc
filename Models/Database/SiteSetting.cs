using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class SiteSetting
{
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public long? UpdatedByUserId { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? UpdatedByUser { get; set; }
}
