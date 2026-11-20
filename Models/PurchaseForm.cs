using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class PurchaseForm
{
    [Required, StringLength(200)] public string ProductName { get; set; } = "";
    public int CategoryId { get; set; }
    [Range(1, 100000)] public int Quantity { get; set; } = 1;
    [Range(0.01, 100000)] public decimal UnitCost { get; set; }
    [Range(0.01, 100000)] public decimal SellingPrice { get; set; }

    [StringLength(400)] public string? StoreLocation { get; set; }
    [StringLength(40)] public string? StoreType { get; set; }
    [StringLength(60)] public string? TargetCustomers { get; set; }
    [StringLength(15)] public string? Season { get; set; }
    [StringLength(15)] public string? SimilarSales { get; set; }
    [Range(0, 1000000)] public int? SimilarStock { get; set; }
    [Range(0, 1000000)] public int? VisitorsPerDay { get; set; }
    public IFormFile? Photo { get; set; }
}

