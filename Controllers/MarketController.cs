using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;

namespace StockMind.Controllers;

public sealed class MarketController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index(string? q, int? category, string? categoryName, string sort = "recommended", bool trends = false, string? offer = null)
    {
        ViewBag.CategoryName = category.HasValue
            ? await db.Categories.Where(c => c.CategoryId == category).Select(c => c.CategoryName).SingleOrDefaultAsync()
            : categoryName;
        ViewBag.Query = q;
        ViewBag.Offer = offer;
        ViewBag.Categories = await db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CategoryName).Select(c => c.CategoryName).ToListAsync();
        ViewBag.Sort = sort;
        ViewBag.Trends = trends;
        ViewBag.Favorites = User.IsInRole("buyer")
            ? (await db.Favorites.Where(f => f.UserId == UserId).Select(f => f.ListingId).ToListAsync()).ToHashSet()
            : new HashSet<long>();
        var listings = Commerce.Available(db);
        if (!string.IsNullOrWhiteSpace(q))
            listings = listings.Where(l => l.Product.ProductName.Contains(q.Trim()) || l.Product.Seller.StoreName.Contains(q.Trim()));
        if (category.HasValue) listings = listings.Where(l => l.Product.CategoryId == category);
        else if (!string.IsNullOrWhiteSpace(categoryName)) listings = listings.Where(l => l.Product.Category.CategoryName == categoryName);

        if (offer is "regular" or "whole_lot") listings = listings.Where(l => l.SellingMethod == offer);

        var sorted = trends ? listings.OrderByDescending(l => l.OrderItems.Where(i => i.Order.OrderStatus != "cancelled").Sum(i => (long?)i.Quantity) ?? 0) : sort switch
        {
            "price-low" => listings.OrderBy(l => l.SellingMethod == "whole_lot" ? l.LotPrice : l.UnitPrice),
            "price-high" => listings.OrderByDescending(l => l.SellingMethod == "whole_lot" ? l.LotPrice : l.UnitPrice),
            "name" => listings.OrderBy(l => l.Product.ProductName),
            _ => listings.OrderByDescending(l => l.PublishedAt)
        };
        return View(await sorted.ThenBy(l => l.ListingId).Take(120).ToListAsync());
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

    public async Task<IActionResult> ProductPhoto(long id)
    {
        var visible = await Commerce.Available(db).AnyAsync(l => l.ProductId == id);
        if (!visible && User.Identity?.IsAuthenticated == true)
            visible = User.IsInRole("admin") || User.IsInRole("seller")
                && await db.Products.AnyAsync(p => p.ProductId == id && p.Seller.UserId == UserId);
        if (!visible) return NotFound();

        var photo = await db.ListingPhotos.AsNoTracking().SingleOrDefaultAsync(p => p.ProductId == id);
        if (photo is null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(photo.Content, photo.ContentType);
    }

    [Authorize(Roles = "buyer")]
    public async Task<IActionResult> Favorites() => View(await Commerce.Available(db)
        .Where(l => l.Favorites.Any(f => f.UserId == UserId)).ToListAsync());

    [Authorize(Roles = "buyer"), HttpPost]
    public async Task<IActionResult> Favorite(long id, bool remove, string? returnUrl)
    {
        await using var transaction = await Commerce.Lock(db);
        var item = await db.Favorites.SingleOrDefaultAsync(f => f.UserId == UserId && f.ListingId == id);
        if (remove && item is not null) db.Favorites.Remove(item);
        if (!remove && item is null && await Commerce.Available(db).AnyAsync(l => l.ListingId == id))
            db.Favorites.Add(new() { UserId = UserId, ListingId = id });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Favorites));
    }
}



