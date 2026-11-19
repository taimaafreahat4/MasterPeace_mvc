using StockMind.Models.Database;

namespace StockMind.Models;

public sealed record BuyerDashboard(string Name, IReadOnlyList<Order> Orders)
{
    public int Completed => Orders.Count(o => o.OrderStatus is "delivered" or "completed");
    public int Pending => Orders.Count(o => o.OrderStatus == "pending");
    public int Processing => Orders.Count(o => o.OrderStatus is "processing" or "shipped");
    public int Cancelled => Orders.Count(o => o.OrderStatus == "cancelled");
    public decimal TotalSpent => Orders.Where(o => o.OrderStatus != "cancelled").Sum(o => o.GrandTotal);
}

public sealed record AdminDashboard(
    int PendingProducts, int ApprovedProducts, int PendingFeedback, int OrderCount,
    IReadOnlyList<Product> ProductQueue, IReadOnlyList<SiteFeedback> FeedbackQueue, IReadOnlyList<Order> RecentOrders);

public sealed record SellerDashboard(IReadOnlyList<Listing> Active, IReadOnlyList<Listing> Listings)
{
    public int Units => Active.Sum(l => l.ListingQuantity);
    public decimal Value => Active.Sum(l => l.SellingMethod == "whole_lot"
        ? (l.LotPrice ?? 0) * (l.ListingQuantity / (l.LotUnits ?? 1))
        : (l.UnitPrice ?? 0) * l.ListingQuantity);
}
