using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class VwPublicListing
{
    public long ListingId { get; set; }

    public long ProductId { get; set; }

    public string SellingMethod { get; set; } = null!;

    public decimal? UnitPrice { get; set; }

    public decimal? LotPrice { get; set; }

    public int? LotUnits { get; set; }

    public int MinOrderQuantity { get; set; }

    public int ListingQuantity { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
