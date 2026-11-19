using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;

namespace StockMind.Controllers;

public sealed class MarketController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index(string? q, int? category, string? categoryName)
    {
        ViewBag.CategoryName = categoryName;
        ViewBag.Categories = await db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.Query = q;
        ViewBag.Category = category;
        var listings = Commerce.Available(db);
        if (!string.IsNullOrWhiteSpace(q)) listings = listings.Where(l => l.Product.ProductName.Contains(q.Trim()));
        if (category.HasValue) listings = listings.Where(l => l.Product.CategoryId == category);
        else if (!string.IsNullOrWhiteSpace(categoryName)) listings = listings.Where(l => l.Product.Category.CategoryName == categoryName);
        return View(await listings.OrderBy(l => l.Product.ProductName).Take(120).ToListAsync());
    }

    public async Task<IActionResult> Details(long id)
    {
        var listing = await Commerce.Available(db).SingleOrDefaultAsync(l => l.ListingId == id);
        if (listing is null) return NotFound();
        ViewBag.Reviews = await db.Reviews.Include(r => r.Reviewer)
            .Where(r => r.ApprovalStatus == "approved" && r.OrderItem.Listing.ProductId == listing.ProductId)
            .OrderByDescending(r => r.CreatedAt).Take(30).ToListAsync();
        return View(listing);
    }

    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Favorites() => View(await Commerce.Available(db)
        .Where(l => l.Favorites.Any(f => f.UserId == UserId)).ToListAsync());

    [Authorize(Roles = "buyer"), HttpPost]
    public async Task<IActionResult> Favorite(long id, bool remove)
    {
        await using var transaction = await Commerce.Lock(db);
        var item = await db.Favorites.SingleOrDefaultAsync(f => f.UserId == UserId && f.ListingId == id);
        if (remove && item is not null) db.Favorites.Remove(item);
        if (!remove && item is null && await Commerce.Available(db).AnyAsync(l => l.ListingId == id))
            db.Favorites.Add(new() { UserId = UserId, ListingId = id });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Favorites));
    }
}

