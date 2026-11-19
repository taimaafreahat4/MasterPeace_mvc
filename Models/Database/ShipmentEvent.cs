using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class ShipmentEvent
{
    public long EventId { get; set; }

    public long ShipmentId { get; set; }

    public string Status { get; set; } = null!;

    public string? EventDescription { get; set; }

    public string? EventLocation { get; set; }

    public DateTime OccurredAt { get; set; }

    public virtual Shipment Shipment { get; set; } = null!;
}
