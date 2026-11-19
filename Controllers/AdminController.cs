using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;

namespace StockMind.Controllers;

[Authorize(Roles = "admin")]
public sealed class AdminController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index()
    {
        var pending = db.Products.AsNoTracking().Where(p => p.ApprovalStatus == "pending" && p.Status != "archived");
        var feedback = db.SiteFeedbacks.AsNoTracking().Where(f => f.ApprovalStatus == "pending");
        return View(new Models.AdminDashboard(
            await pending.CountAsync(),
            await db.Products.CountAsync(p => p.ApprovalStatus == "approved" && p.Status != "archived"),
            await feedback.CountAsync(),
            await db.Orders.CountAsync(),
            await pending.Include(p => p.Category).Include(p => p.Seller).OrderByDescending(p => p.SubmittedAt).Take(5).ToListAsync(),
            await feedback.OrderByDescending(f => f.SubmittedAt).Take(4).ToListAsync(),
            await db.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt).Take(5).ToListAsync()));
    }

    public async Task<IActionResult> Sellers() => View(await db.SellerProfiles.Include(s => s.User)
        .OrderByDescending(s => s.CreatedAt).Take(200).ToListAsync());

    public async Task<IActionResult> Products(string? q, string approval = "all")
    {
        await LoadModerationTotals();
        ViewBag.Query = q;
        ViewBag.Approval = approval;
        var products = db.Products.Include(p => p.Seller).Include(p => p.Category).Include(p => p.Listings)
            .Where(p => p.Status != "archived");
        if (!string.IsNullOrWhiteSpace(q))
            products = products.Where(p => p.ProductName.Contains(q) || p.Seller.StoreName.Contains(q) || p.Category.CategoryName.Contains(q));
        if (approval is "pending" or "approved" or "rejected")
            products = products.Where(p => p.ApprovalStatus == approval);
        return View(await products.OrderBy(p => p.ApprovalStatus != "pending").ThenByDescending(p => p.CreatedAt).Take(200).ToListAsync());
    }

    private async Task LoadModerationTotals()
    {
        ViewBag.PendingProducts = await db.Products.CountAsync(p => p.Status != "archived" && p.ApprovalStatus == "pending");
        ViewBag.ApprovedProducts = await db.Products.CountAsync(p => p.Status != "archived" && p.ApprovalStatus == "approved");
        ViewBag.PendingFeedback = await db.SiteFeedbacks.CountAsync(f => f.ApprovalStatus == "pending");
        ViewBag.PublicFeedback = await db.SiteFeedbacks.CountAsync(f => f.ApprovalStatus == "approved");
    }

    [HttpPost]
    public async Task<IActionResult> Moderate(long id, bool approve, string? reason)
    {
        if (reason?.Length > 1000 || (!approve && string.IsNullOrWhiteSpace(reason))) return BadRequest();
        await using var transaction = await Commerce.Lock(db);
        var product = await db.Products.Include(p => p.Seller).Include(p => p.Listings)
            .SingleOrDefaultAsync(p => p.ProductId == id && p.Status != "archived");
        if (product is null) return NotFound();
        if (approve && product.Seller.VerificationStatus != "approved")
            return Notice("seller-pending", "Admin", nameof(Products));
        product.ApprovalStatus = approve ? "approved" : "rejected";
        product.Status = approve ? "active" : "inactive";
        product.ReviewedAt = DateTime.UtcNow;
        product.ReviewedByUserId = UserId;
        product.RejectionReason = approve ? null : reason;
        var latest = product.Listings.MaxBy(l => l.ListingId);
        foreach (var listing in product.Listings)
        {
            listing.Status = approve && listing == latest ? "active" : "paused";
            if (listing.Status == "active") listing.PublishedAt = DateTime.UtcNow;
        }
        db.ModerationActions.Add(new() { ModeratorUserId = UserId, ProductId = id, Reason = reason, ActionType = approve ? "approve" : "reject" });
        db.Notifications.Add(new() { UserId = product.Seller.UserId, Title = "Product review",
            Body = $"{product.ProductName}: {product.ApprovalStatus}", NotificationType = "product" });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Users() => View(await db.Users.Include(u => u.Role).Include(u => u.SellerProfile)
        .OrderByDescending(u => u.CreatedAt).Take(200).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> UserStatus(long id, bool active)
    {
        var member = await db.Users.SingleOrDefaultAsync(u => u.UserId == id && u.Role.RoleName != "admin");
        if (member is null) return NotFound();
        member.AccountStatus = active ? "active" : "suspended";
        member.UpdatedAt = DateTime.UtcNow;
        db.AuditLogs.Add(new() { ActorUserId = UserId, EntityId = id.ToString(), EntityName = "Users", ActionName = member.AccountStatus });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public async Task<IActionResult> Verify(long id, bool approve, bool returnToSellers = false)
    {
        await using var transaction = await Commerce.Lock(db);
        var seller = await db.SellerProfiles.FindAsync(id);
        if (seller is null) return NotFound();
        seller.VerificationStatus = approve ? "approved" : "rejected";
        db.Notifications.Add(new() { UserId = seller.UserId, Title = "Store verification",
            Body = $"Your store verification is {seller.VerificationStatus}.", NotificationType = "seller" });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(returnToSellers ? nameof(Sellers) : nameof(Users));
    }

    public async Task<IActionResult> Support()
    {
        await LoadModerationTotals();
        ViewBag.Feedback = await db.SiteFeedbacks.OrderBy(f => f.ApprovalStatus != "pending").ThenByDescending(f => f.SubmittedAt).Take(100).ToListAsync();
        ViewBag.Reviews = await db.Reviews.Include(r => r.OrderItem).OrderBy(r => r.ApprovalStatus != "pending")
            .ThenByDescending(r => r.CreatedAt).Take(100).ToListAsync();
        return View(await db.SupportTickets.OrderBy(t => t.TicketStatus != "open").ThenByDescending(t => t.CreatedAt).Take(100).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Close(long id)
    {
        var ticket = await db.SupportTickets.FindAsync(id);
        if (ticket is null) return NotFound();
        ticket.TicketStatus = "closed";
        ticket.AssignedAdminId = UserId;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Support));
    }

    [HttpPost]
    public async Task<IActionResult> ModerateFeedback(long id, bool approve, bool review)
    {
        if (review)
        {
            var row = await db.Reviews.FindAsync(id);
            if (row is null) return NotFound();
            row.ApprovalStatus = approve ? "approved" : "rejected";
            row.ReviewedAt = DateTime.UtcNow;
            row.ReviewedByUserId = UserId;
        }
        else
        {
            var row = await db.SiteFeedbacks.FindAsync(id);
            if (row is null) return NotFound();
            row.ApprovalStatus = approve ? "approved" : "rejected";
            row.ReviewedAt = DateTime.UtcNow;
            row.ReviewedByUserId = UserId;
        }
        db.ModerationActions.Add(new() { ModeratorUserId = UserId,
            ReviewId = review ? id : null, FeedbackId = review ? null : id, ActionType = approve ? "approve" : "reject" });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Support));
    }

    [HttpPost]
    public async Task<IActionResult> Ship(long id, string? tracking)
    {
        if (tracking?.Length > 100) return BadRequest();
        await using var transaction = await Commerce.Lock(db);
        var shipment = await db.Shipments.Include(s => s.Order.Shipments)
            .SingleOrDefaultAsync(s => s.ShipmentId == id);
        if (shipment is null) return NotFound();
        var now = DateTime.UtcNow;
        if (shipment.ShipmentStatus == "processing")
        {
            shipment.ShipmentStatus = "shipped";
            shipment.ShippedAt = now;
            shipment.TrackingNumber = tracking;
        }
        else if (shipment.ShipmentStatus == "shipped")
        {
            shipment.ShipmentStatus = "delivered";
            shipment.DeliveredAt = now;
        }
        else return RedirectToAction("Details", "Orders", new { id = shipment.OrderId });

        var order = shipment.Order;
        order.OrderStatus = order.Shipments.All(s => s.ShipmentStatus == "delivered") ? "delivered"
            : order.Shipments.Any(s => s.ShipmentStatus == "processing") ? "processing" : "shipped";
        shipment.ShipmentEvents.Add(new() { Status = shipment.ShipmentStatus,
            EventDescription = shipment.ShipmentStatus == "shipped" ? "Shipment dispatched" : "Shipment delivered", OccurredAt = now });
        if (order.BuyerId is long buyerId && !await db.UserPreferences.AnyAsync(p => p.UserId == buyerId && !p.ShippingNotifications))
            db.Notifications.Add(new() { UserId = buyerId, Title = "Shipment update",
                Body = $"Order {order.OrderNumber}: shipment {shipment.ShipmentStatus}.", NotificationType = "shipping" });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction("Details", "Orders", new { id = shipment.OrderId });
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmCash(long id)
    {
        var payment = await db.Payments.Include(p => p.Order).SingleOrDefaultAsync(p => p.PaymentId == id
            && p.OrderId != null && p.Order!.OrderStatus == "delivered" && p.PaymentMethod == "cash_on_delivery");
        if (payment is null) return NotFound();
        if (payment.PaymentStatus == "pending")
        {
            payment.PaymentStatus = "paid";
            payment.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return RedirectToAction("Details", "Orders", new { id = payment.OrderId });
    }

    public async Task<IActionResult> Subscriptions() => View(await db.Payments.Include(p => p.Subscription!.Plan)
        .Include(p => p.Subscription!.Seller).Where(p => p.SubscriptionId != null)
        .OrderByDescending(p => p.CreatedAt).Take(100).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> ConfirmSubscription(long id)
    {
        await using var transaction = await Commerce.Lock(db);
        var payment = await db.Payments.Include(p => p.Subscription!.Plan)
            .SingleOrDefaultAsync(p => p.PaymentId == id && p.SubscriptionId != null && p.PaymentStatus == "pending");
        if (payment?.Subscription is not { Status: "scheduled" } subscription) return NotFound();
        var now = DateTime.UtcNow;
        var end = await db.Subscriptions.Where(s => s.SellerId == subscription.SellerId && s.SubscriptionId != subscription.SubscriptionId
            && s.Status == "active" && s.EndAt > now).MaxAsync(s => (DateTime?)s.EndAt);
        subscription.StartAt = end ?? now;
        subscription.EndAt = subscription.StartAt.AddMonths(subscription.Plan.DurationMonths);
        subscription.PricePaid = payment.Amount;
        subscription.Status = "active";
        payment.PaymentStatus = "paid";
        payment.PaidAt = now;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Subscriptions));
    }

    public async Task<IActionResult> Settings() => View(await db.SiteSettings.OrderBy(s => s.SettingKey).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Settings(string? key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100 || value?.Length > 2000) return BadRequest();
        var setting = await db.SiteSettings.FindAsync(key);
        if (setting is null) return NotFound();
        setting.SettingValue = value;
        setting.UpdatedByUserId = UserId;
        setting.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Settings));
    }
}





