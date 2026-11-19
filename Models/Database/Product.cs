using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Product
{
    public long ProductId { get; set; }

    public long SellerId { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public string ProductCondition { get; set; } = null!;

    public string ApprovalStatus { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public long? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public string? Sku { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Inventory? Inventory { get; set; }

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual ICollection<ModerationAction> ModerationActions { get; set; } = new List<ModerationAction>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<RecommendationAnalysis> RecommendationAnalyses { get; set; } = new List<RecommendationAnalysis>();

    public virtual User? ReviewedByUser { get; set; }

    public virtual SellerProfile Seller { get; set; } = null!;
}

