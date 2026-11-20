using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

[Authorize(Roles = "seller")]
public sealed class SurplusController(StockMindDb db, IWebHostEnvironment environment) : StockController
{
    private IQueryable<RecommendationAnalysis> Mine => db.RecommendationAnalyses.AsNoTracking().Where(r => r.Seller.UserId == UserId);
    private IQueryable<Product> Products => db.Products.AsNoTracking().Where(p => p.Seller.UserId == UserId && p.Status != "archived");

    public async Task<IActionResult> Index(long? productId)
    {
        var seller = await SellerSubscriptions.SellerId(db, UserId);
        if (!await SellerSubscriptions.CanRecommend(db, seller, environment.IsDevelopment())) return ChoosePlan(productId);
        var form = new SurplusForm();
        if (productId is long id)
        {
            var product = await Products.Include(p => p.Inventory).Include(p => p.Listings).SingleOrDefaultAsync(p => p.ProductId == id);
            if (product is null) return NotFound();
            var listing = product.Listings.MaxBy(l => l.ListingId);
            var since = DateTime.UtcNow.AddDays(-30);
            form = new SurplusForm
            {
                Name = product.ProductName, CategoryId = product.CategoryId, Quantity = product.Inventory?.AvailableQuantity ?? 0,
                Price = listing is null ? 0 : Commerce.Price(listing) / Commerce.UnitsPerPurchase(listing),
                AgeDays = Math.Clamp((DateTime.UtcNow - (listing?.PublishedAt ?? product.CreatedAt)).Days, 0, 3650),
                RecentSales = await db.OrderItems.Where(i => i.Listing.ProductId == id && i.Order.CreatedAt >= since
                    && i.Order.OrderStatus == "delivered").SumAsync(i => (int?)i.Quantity * (i.UnitsPerLot ?? 1)) ?? 0
            };
        }
        return await Show(form, productId);
    }

    [HttpPost, RequestSizeLimit(1_700_000)]
    public async Task<IActionResult> Index(SurplusForm form, long? productId, CancellationToken cancellationToken)
    {
        var seller = await SellerSubscriptions.SellerId(db, UserId, cancellationToken);
        if (!await SellerSubscriptions.CanRecommend(db, seller, environment.IsDevelopment(), cancellationToken)) return ChoosePlan(productId);
        if (productId.HasValue && !await Products.AnyAsync(p => p.ProductId == productId, cancellationToken)) return NotFound();
        if (!await db.Categories.AnyAsync(c => c.IsActive && c.CategoryId == form.CategoryId, cancellationToken))
            ModelState.AddModelError(nameof(form.CategoryId), "Choose an active category.");
        var (photo, type) = await SellerAnalysisPhoto.ReadAsync(form.Photo, cancellationToken);
        if (form.Photo is not null && type is null)
            ModelState.AddModelError(nameof(form.Photo), "Choose a JPG, PNG or WEBP image up to 1.5 MB.");
        if (!ModelState.IsValid) return await Show(form, productId);

        await using var transaction = await Commerce.Lock(db);
        if (!await SellerSubscriptions.HasPremium(db, seller, DateTime.UtcNow, environment.IsDevelopment(), cancellationToken))
        {
            var free = await db.Set<SellerFreePlan>().FindAsync([seller], cancellationToken);
            if (free is null || free.RecommendationUsedAt is not null) return ChoosePlan(productId);
            free.RecommendationUsedAt = DateTime.UtcNow;
        }
        int? midpoint = form.RecentSales is int units ? (int)Math.Round(units * form.HorizonDays / 30m) : null;
        var risk = midpoint is null ? "Sales data needed" : midpoint == 0 || form.Quantity > midpoint * 2 ? "High"
            : form.Quantity > midpoint || form.AgeDays >= 90 ? "Moderate" : "Low";
        var action = midpoint is null ? "Add recent sales information" : risk == "High" ? "Promote this bulk lot"
            : risk == "Moderate" ? "Review price and listing visibility" : "Monitor sales and stock";
        var forecast = new SurplusForecast(form.RecentSales, form.HorizonDays, midpoint,
            midpoint is int value ? (int)Math.Floor(value * .8m) : null,
            midpoint is int high ? (int)Math.Ceiling(high * 1.2m) : null, risk);
        var row = new RecommendationAnalysis
        {
            SellerId = seller, InventoryProductId = productId, ProductName = form.Name.Trim(),
            QuantityOnHand = form.Quantity, SalesPerWeek = form.RecentSales * 7m / 30m,
            SuggestedAction = action, Photo = photo, PhotoType = type, ForecastJson = JsonSerializer.Serialize(forecast),
            AnalysisJson = JsonSerializer.Serialize(new { form.CategoryId, form.Price, form.Condition, form.AgeDays })
        };
        db.RecommendationAnalyses.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(Report), new { id = row.RecommendationId });
    }

    public async Task<IActionResult> Report(long id)
    {
        var analysis = await Mine.SingleOrDefaultAsync(r => r.RecommendationId == id);
        if (analysis is null) return NotFound();
        ViewBag.Forecast = SurplusForecast.Read(analysis.ForecastJson);
        return View(analysis);
    }

    public async Task<IActionResult> Photo(long id)
    {
        var image = await Mine.Where(r => r.RecommendationId == id).Select(r => new { r.Photo, r.PhotoType }).SingleOrDefaultAsync();
        if (image?.Photo is null || image.PhotoType is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(image.Photo, image.PhotoType);
    }

    private IActionResult ChoosePlan(long? productId) => RedirectToAction("Plans", "Seller", new { productId, required = true });

    private async Task<IActionResult> Show(SurplusForm form, long? productId)
    {
        ViewBag.ProductId = productId;
        ViewBag.Categories = await db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.Listings = await Products.OrderBy(p => p.ProductName).Select(p => new SellerAnalysisSummary(p.ProductId, p.ProductName, null)).ToListAsync();
        ViewBag.History = await Mine.OrderByDescending(r => r.RecommendationId)
            .Select(r => new SellerAnalysisSummary(r.RecommendationId, r.ProductName, r.SuggestedAction)).Take(20).ToListAsync();
        return View("Index", form);
    }
}
