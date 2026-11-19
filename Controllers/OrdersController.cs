using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;

namespace StockMind.Controllers;

[Authorize(Roles = "buyer,admin")]
public sealed class OrdersController(StockMindDb db) : StockController
{
    private IQueryable<Models.Database.Order> Orders => db.Orders.Where(o => User.IsInRole("admin") || o.BuyerId == UserId);

    public async Task<IActionResult> Index() => View(await Orders.OrderByDescending(o => o.CreatedAt).Take(100).ToListAsync());

    public async Task<IActionResult> Details(long id)
    {
        var order = await Orders.Include(o => o.OrderItems).ThenInclude(i => i.Review)
            .Include(o => o.Payments).Include(o => o.Shipments).ThenInclude(s => s.ShipmentEvents)
            .AsSplitQuery().SingleOrDefaultAsync(o => o.OrderId == id);
        return order is null ? NotFound() : View(order);
    }

    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Confirmation(long id)
    {
        var order = await Orders.AsNoTracking().Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.OrderId == id);
        return order is null ? NotFound() : View(order);
    }

    public async Task<IActionResult> Track(long id)
    {
        var order = await Orders.AsNoTracking().Include(o => o.Shipments).ThenInclude(s => s.ShipmentEvents)
            .SingleOrDefaultAsync(o => o.OrderId == id);
        return order is null ? NotFound() : View(order);
    }

    [HttpGet("/checkout/tracking/tracking.html")]
    [HttpGet("/checkout/confirmation/confirmation.html")]
    [HttpGet("/checkout/feedback/feedback.html")]
    public async Task<IActionResult> LegacyOrder(string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return RedirectToAction(nameof(Index));
        var numeric = long.TryParse(order, out var id);
        var match = await Orders.AsNoTracking()
            .Where(o => o.OrderNumber == order || (numeric && o.OrderId == id))
            .Select(o => (long?)o.OrderId).FirstOrDefaultAsync();
        if (match is null) return NotFound();

        var path = Request.Path.Value ?? "";
        var action = path.Contains("/tracking/", StringComparison.OrdinalIgnoreCase) ? nameof(Track)
            : path.Contains("/confirmation/", StringComparison.OrdinalIgnoreCase) && User.IsInRole("buyer") ? nameof(Confirmation)
            : nameof(Details);
        return RedirectToAction(action, new { id = match.Value });
    }

    [Authorize(Roles = "buyer"), HttpPost]
    public async Task<IActionResult> Review(long id, byte rating, string? comment)
    {
        if (rating is < 1 or > 5 || comment?.Length > 2000) return BadRequest();
        await using var transaction = await Commerce.Lock(db);
        var item = await db.OrderItems.SingleOrDefaultAsync(i => i.OrderItemId == id
            && i.Order.BuyerId == UserId && i.Order.OrderStatus == "delivered");
        if (item is null) return NotFound();
        if (!await db.Reviews.AnyAsync(r => r.OrderItemId == id && r.ReviewerId == UserId))
        {
            db.Reviews.Add(new() { OrderItemId = id, ReviewerId = UserId, Rating = rating,
                Comment = comment, ApprovalStatus = "pending" });
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Details), new { id = item.OrderId });
    }
}



