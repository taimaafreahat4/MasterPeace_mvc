using System.ComponentModel.DataAnnotations;
using StockMind.Models.Database;

namespace StockMind.Models;

public sealed class SellerFreePlan
{
    public long SellerId { get; set; }
    public DateTime? RecommendationUsedAt { get; set; }
}

public sealed class SubscriptionCheckout
{
    public Guid CheckoutId { get; set; }
    public long SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? StoreName { get; set; }
    public string Country { get; set; } = "";
    public bool IsDemo { get; set; }
}

public sealed class SubscriptionCheckoutForm
{
    public Guid CheckoutId { get; set; }
    public int PlanId { get; set; }
    public long? ProductId { get; set; }
    [Required, StringLength(150)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(255)] public string Email { get; set; } = "";
    [StringLength(180)] public string? StoreName { get; set; }
    [Required, StringLength(100)] public string Country { get; set; } = "";
    public bool Confirm { get; set; }
}

public sealed record SellerPlans(IReadOnlyList<SubscriptionPlan> Plans, IReadOnlyList<Subscription> History,
    bool FreeUsed, long? ProductId, IReadOnlyList<SellerAnalysisSummary> Reports);
