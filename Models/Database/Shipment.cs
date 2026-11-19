using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Shipment
{
    public long ShipmentId { get; set; }

    public long OrderId { get; set; }

    public long? SellerId { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Carrier { get; set; }

    public string ShipmentStatus { get; set; } = null!;

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual SellerProfile? Seller { get; set; }

    public virtual ICollection<ShipmentEvent> ShipmentEvents { get; set; } = new List<ShipmentEvent>();
}
