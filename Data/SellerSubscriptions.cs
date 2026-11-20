using Microsoft.EntityFrameworkCore;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Data;

internal static class SellerSubscriptions
{
    internal static IQueryable<SubscriptionPlan> Plans(StockMindDb db) => db.SubscriptionPlans.AsNoTracking()
        .Where(p => p.IsActive && (p.PlanCode == "PREMIUM_1" || p.PlanCode == "PREMIUM_3" || p.PlanCode == "PREMIUM_6"))
        .OrderBy(p => p.DurationMonths);

    internal static IQueryable<Subscription> Premium(StockMindDb db, long sellerId, bool allowDemo) => db.Subscriptions
        .Where(s => s.SellerId == sellerId && s.Status == "active" && s.Plan.Price > 0
            && (allowDemo || !s.Payments.Any(p => p.PaymentMethod == "demo")));

    internal static Task<bool> HasPremium(StockMindDb db, long sellerId, DateTime now, bool allowDemo, CancellationToken token = default)
        => Premium(db, sellerId, allowDemo).AnyAsync(s => s.StartAt <= now && s.EndAt > now, token);

    internal static Task<long> SellerId(StockMindDb db, long userId, CancellationToken token = default)
        => db.SellerProfiles.Where(s => s.UserId == userId).Select(s => s.SellerId).SingleAsync(token);

    internal static async Task<bool> CanRecommend(StockMindDb db, long sellerId, bool allowDemo, CancellationToken token = default)
        => await HasPremium(db, sellerId, DateTime.UtcNow, allowDemo, token)
            || await db.Set<SellerFreePlan>().AnyAsync(f => f.SellerId == sellerId && f.RecommendationUsedAt == null, token);
}
