using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class OrderItem
{
    public long OrderItemId { get; set; }

    public long OrderId { get; set; }

    public long ListingId { get; set; }

    public long SellerId { get; set; }

    public string ProductNameSnapshot { get; set; } = null!;

    public string SellingMethodSnapshot { get; set; } = null!;

    public int Quantity { get; set; }

    public int? UnitsPerLot { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Review? Review { get; set; }

    public virtual SellerProfile Seller { get; set; } = null!;
}
