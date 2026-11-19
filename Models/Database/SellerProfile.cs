using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class SellerProfile
{
    public long SellerId { get; set; }

    public long UserId { get; set; }

    public string StoreName { get; set; } = null!;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? LocationDetails { get; set; }

    public string? BusinessCategory { get; set; }

    public string? StoreType { get; set; }

    public string? TargetCustomers { get; set; }

    public int? AverageVisitorsPerDay { get; set; }

    public string VerificationStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AdvisorAnalysis> AdvisorAnalyses { get; set; } = new List<AdvisorAnalysis>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<RecommendationAnalysis> RecommendationAnalyses { get; set; } = new List<RecommendationAnalysis>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual User User { get; set; } = null!;
}
