using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class RecommendationAnalysis
{
    public long RecommendationId { get; set; }

    public long SellerId { get; set; }

    public long? InventoryProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductImageUrl { get; set; }

    public int? QuantityOnHand { get; set; }

    public decimal? SalesPerWeek { get; set; }

    public string? SuggestedAction { get; set; }

    public decimal? SuggestedDiscountPct { get; set; }

    public string? ForecastJson { get; set; }

    public string? AnalysisJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product? InventoryProduct { get; set; }

    public virtual SellerProfile Seller { get; set; } = null!;
}
