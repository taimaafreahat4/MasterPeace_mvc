using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class CartItem
{
    public long CartItemId { get; set; }

    public long CartId { get; set; }

    public long ListingId { get; set; }

    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Listing Listing { get; set; } = null!;
}
