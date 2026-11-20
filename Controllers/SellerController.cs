using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

[Authorize(Roles = "seller")]
public sealed class SellerController(StockMindDb db) : StockController
{
    private IQueryable<Product> Products => db.Products.Where(p => p.Seller.UserId == UserId)
        .Include(p => p.Inventory).Include(p => p.Listings).Include(p => p.Category);

    public async Task<IActionResult> Index()
    {
        var listings = await db.Listings.AsNoTracking().Include(l => l.Product.Category)
            .Where(l => l.Product.Seller.UserId == UserId && l.Product.Status != "archived" && l.Status != "archived")
            .OrderByDescending(l => l.ListingId).ToListAsync();
        var active = await Commerce.Available(db).Where(l => l.Product.Seller.UserId == UserId && l.ListingQuantity > 0).ToListAsync();
        return View(new SellerDashboard(active, listings));
    }

    public async Task<IActionResult> Listings()
    {
        ViewBag.Seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        return View(await Products.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Edit(long id = 0)
    {
        await Categories();
        if (id == 0) return View(new ProductForm());
        var product = await Products.SingleOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();
        var listing = Latest(product);
        return View(new ProductForm
        {
            Id = id, Name = product.ProductName, Description = product.Description, CategoryId = product.CategoryId,
            Price = listing?.UnitPrice ?? listing?.LotPrice ?? 0, Quantity = product.Inventory?.AvailableQuantity ?? 0,
            Minimum = listing?.MinOrderQuantity ?? 1, WholeLot = listing?.SellingMethod == "whole_lot",
            LotUnits = listing?.LotUnits ?? 1, ReorderLevel = product.Inventory?.ReorderLevel ?? 0,
            ImageUrl = await db.ProductImages.Where(i => i.ProductId == id && i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefaultAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductForm form)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == form.CategoryId && c.IsActive))
            ModelState.AddModelError(nameof(form.CategoryId), "Choose an active category.");
        if (!string.IsNullOrWhiteSpace(form.ImageUrl) && !SafeImage(form.ImageUrl))
            ModelState.AddModelError(nameof(form.ImageUrl), "Use an HTTPS image link.");
        if (!ModelState.IsValid) { await Categories(); return View(form); }

        await using var transaction = await Commerce.Lock(db);
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        var product = form.Id == 0 ? new Product { SellerId = seller.SellerId }
            : await Products.SingleOrDefaultAsync(p => p.ProductId == form.Id);
        if (product is null) return NotFound();
        if (form.Id == 0) db.Products.Add(product);
        product.ProductName = form.Name.Trim();
        product.Description = form.Description;
        product.CategoryId = form.CategoryId;
        product.Status = "draft";
        product.ApprovalStatus = "pending";
        product.SubmittedAt = product.UpdatedAt = DateTime.UtcNow;
        product.ReviewedAt = null;
        product.ReviewedByUserId = null;
        product.RejectionReason = null;

        var inventory = product.Inventory ??= new Inventory();
        var difference = form.Quantity - inventory.AvailableQuantity;
        inventory.AvailableQuantity = form.Quantity;
        inventory.ReorderLevel = form.ReorderLevel;
        inventory.LastUpdated = DateTime.UtcNow;
        if (difference != 0) inventory.InventoryMovements.Add(new()
        { QuantityChange = difference, MovementType = "adjustment", Reason = "Seller stock update" });

        var listing = Latest(product);
        if (listing is null)
        {
            listing = new Listing();
            product.Listings.Add(listing);
        }
        listing.Status = "draft";
        listing.ExpiresAt = null;
        listing.SellingMethod = form.WholeLot ? "whole_lot" : "regular";
        listing.UnitPrice = form.WholeLot ? null : form.Price;
        listing.LotPrice = form.WholeLot ? form.Price : null;
        listing.LotUnits = form.WholeLot ? form.LotUnits : null;
        listing.MinOrderQuantity = form.WholeLot ? 1 : form.Minimum;
        listing.ListingQuantity = form.Quantity;

        var image = form.Id == 0 ? null : await db.ProductImages.FirstOrDefaultAsync(i => i.ProductId == form.Id && i.IsPrimary);
        if (image is not null)
        {
            if (string.IsNullOrWhiteSpace(form.ImageUrl)) db.ProductImages.Remove(image);
            else image.ImageUrl = form.ImageUrl;
        }
        else if (!string.IsNullOrWhiteSpace(form.ImageUrl))
            db.ProductImages.Add(new() { Product = product, ImageUrl = form.ImageUrl, IsPrimary = true });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Notice("product-submitted", "Seller", nameof(Listings));
    }

    [HttpPost]
    public async Task<IActionResult> Archive(long id)
    {
        await using var transaction = await Commerce.Lock(db);
        var product = await Products.SingleOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();
        product.Status = "archived";
        product.UpdatedAt = DateTime.UtcNow;
        foreach (var listing in product.Listings) listing.Status = "archived";
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sales() => View(await db.OrderItems.Include(i => i.Order)
        .Where(i => i.Seller.UserId == UserId).OrderByDescending(i => i.Order.CreatedAt).Take(100).ToListAsync());

    public async Task<IActionResult> Plans()
    {
        ViewBag.Plans = await db.SubscriptionPlans.Include(p => p.Features).Where(p => p.IsActive).ToListAsync();
        return View(await db.Subscriptions.Include(s => s.Plan).Where(s => s.Seller.UserId == UserId)
            .OrderByDescending(s => s.EndAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> RequestPlan(int id)
    {
        await using var transaction = await Commerce.Lock(db);
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        if (seller.VerificationStatus != "approved") return Forbid();
        var plan = await db.SubscriptionPlans.SingleOrDefaultAsync(p => p.PlanId == id && p.IsActive);
        if (plan is null) return NotFound();
        if (await db.Subscriptions.AnyAsync(s => s.SellerId == seller.SellerId && s.Status == "scheduled"))
            return Notice("plan-scheduled", "Seller", nameof(Plans));
        var now = DateTime.UtcNow;
        var end = await db.Subscriptions.Where(s => s.SellerId == seller.SellerId && s.Status == "active" && s.EndAt > now)
            .MaxAsync(s => (DateTime?)s.EndAt);
        var start = end ?? now;
        var subscription = new Subscription { SellerId = seller.SellerId, PlanId = plan.PlanId, Status = "scheduled",
            StartAt = start, EndAt = start.AddMonths(plan.DurationMonths), PricePaid = 0 };
        db.Subscriptions.Add(subscription);
        db.Payments.Add(new() { Subscription = subscription, UserId = UserId, Amount = plan.Price, Currency = plan.Currency,
            PaymentMethod = "manual", PaymentStatus = "pending" });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Notice("plan-requested", "Seller", nameof(Plans));
    }

    public async Task<IActionResult> StoreProfile()
    {
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        return View(new StoreProfileForm
        {
            StoreName = seller.StoreName, BusinessCategory = seller.BusinessCategory ?? "",
            LocationDetails = seller.LocationDetails ?? "", City = seller.City ?? "",
            Country = seller.Country ?? "", StoreType = seller.StoreType ?? "",
            TargetCustomers = seller.TargetCustomers ?? "", AverageVisitorsPerDay = seller.AverageVisitorsPerDay,
            Description = seller.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> StoreProfile(StoreProfileForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        seller.StoreName = form.StoreName.Trim();
        seller.BusinessCategory = form.BusinessCategory;
        seller.LocationDetails = form.LocationDetails;
        seller.City = form.City;
        seller.Country = form.Country;
        seller.StoreType = form.StoreType;
        seller.TargetCustomers = form.TargetCustomers;
        seller.AverageVisitorsPerDay = form.AverageVisitorsPerDay;
        seller.Description = form.Description;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Tools()
    {
        var now = DateTime.UtcNow;
        ViewBag.PremiumActive = await db.Subscriptions.AnyAsync(s =>
            s.Seller.UserId == UserId && s.Status == "active" && s.StartAt <= now && s.EndAt > now);
        return View();
    }

    public IActionResult Insights() => RedirectToAction("Index", "Surplus");

    [HttpPost]
    public IActionResult Analyze(long id) => RedirectToAction("Index", "Surplus", new { productId = id });

    private static Listing? Latest(Product p) => p.Listings.MaxBy(l => l.ListingId);
    private Task Categories() => FillCategories();
    private async Task FillCategories() => ViewBag.Categories = await db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
    internal static bool SafeImage(string url) => (url.StartsWith("/catalog/", StringComparison.Ordinal) && !url.Contains("..") && !url.Contains('\\'))
        || Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "https";
}





