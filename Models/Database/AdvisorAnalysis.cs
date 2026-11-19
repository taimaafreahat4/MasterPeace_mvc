using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class AdvisorAnalysis
{
    public long AnalysisId { get; set; }

    public long SellerId { get; set; }

    public string ProductName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public string? ProductImageUrl { get; set; }

    public int? PurchaseQuantity { get; set; }

    public decimal? PurchaseUnitCost { get; set; }

    public decimal? ExpectedSellingPrice { get; set; }

    public string? StoreLocation { get; set; }

    public string? BusinessCategory { get; set; }

    public string? StoreType { get; set; }

    public string? TargetCustomers { get; set; }

    public int? VisitorsPerDay { get; set; }

    public string? Recommendation { get; set; }

    public decimal? RiskScore { get; set; }

    public string? AnalysisJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual SellerProfile Seller { get; set; } = null!;
}
