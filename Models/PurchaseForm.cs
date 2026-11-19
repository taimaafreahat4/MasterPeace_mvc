using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class PurchaseForm
{
    [Required, StringLength(200)] public string ProductName { get; set; } = "";
    public int CategoryId { get; set; }
    [Range(1, 100000)] public int Quantity { get; set; } = 1;
    [Range(0.01, 100000)] public decimal UnitCost { get; set; }
    [Range(0.01, 100000)] public decimal SellingPrice { get; set; }
}
