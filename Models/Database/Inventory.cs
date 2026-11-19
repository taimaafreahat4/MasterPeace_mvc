using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Inventory
{
    public long InventoryId { get; set; }

    public long ProductId { get; set; }

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public decimal? UnitCost { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual Product Product { get; set; } = null!;
}
