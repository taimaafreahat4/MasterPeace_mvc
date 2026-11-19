using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class InventoryMovement
{
    public long MovementId { get; set; }

    public long InventoryId { get; set; }

    public string MovementType { get; set; } = null!;

    public int QuantityChange { get; set; }

    public string? Reason { get; set; }

    public long? ReferenceOrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Inventory Inventory { get; set; } = null!;

    public virtual Order? ReferenceOrder { get; set; }
}
