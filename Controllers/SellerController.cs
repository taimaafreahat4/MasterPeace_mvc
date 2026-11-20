using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

[Authorize(Roles = "seller")]
public sealed partial class SellerController(StockMindDb db, IWebHostEnvironment environment) : StockController
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

    public async Task<IActionResult> Listings(string? q, int? category, string? offer, bool add = false, long? edit = null)
    {
        ProductForm? form = add ? new ProductForm() : null;
        string? photo = null;
        if (edit.HasValue)
        {
            var product = await Products.SingleOrDefaultAsync(p => p.ProductId == edit && p.Status != "archived");
            if (product is null) return NotFound();
            var listing = Latest(product);
            photo = await PrimaryImage(product.ProductId).Select(i => i.ImageUrl).FirstOrDefaultAsync();
            form = new ProductForm
            {
                Id = product.ProductId, Name = product.ProductName, Description = product.Description,
                CategoryId = product.CategoryId, Condition = product.ProductCondition ?? "New",
                Price = listing?.UnitPrice ?? listing?.LotPrice ?? 0, Quantity = product.Inventory?.AvailableQuantity ?? 0,
                Minimum = listing?.MinOrderQuantity ?? 1, WholeLot = listing?.SellingMethod == "whole_lot",
                LotUnits = listing?.LotUnits, ReorderLevel = product.Inventory?.ReorderLevel ?? 0,
                ImageUrl = photo == PhotoUrl(product.ProductId) ? null : photo
            };
        }
        return await Marketplace(form, q, category, offer, photo);
    }

    private async Task<IActionResult> Marketplace(ProductForm? form, string? q, int? category, string? offer, string? photo)
    {
        var seller = await db.SellerProfiles.AsNoTracking().SingleAsync(s => s.UserId == UserId);
        var categories = await db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        var categoryNames = seller.BusinessCategory?.Trim().ToLowerInvariant() switch
        {
            "home" => new[] { "Home & Living", "Home & Kitchen" },
            "clothes" or "fashion" => new[] { "Clothing" },
            "books" => new[] { "Books", "Office Supplies" },
            "toys_baby" => new[] { "Toys & Baby" },
            "cosmetics" => new[] { "Cosmetics", "Beauty & Personal Care" },
            "accessories" => new[] { "Bags & Accessories", "Accessories" },
            "grocery" => new[] { "Groceries" },
            var categoryName => new[] { categoryName }
        };
        var storeCategory = categories.FirstOrDefault(c => categoryNames.Contains(c.CategoryName, StringComparer.OrdinalIgnoreCase));
        var selectedCategory = category ?? storeCategory?.CategoryId ?? 0;
        q = q?.Trim();
        offer = offer is "regular" or "whole_lot" ? offer : null;

        var own = db.Listings.AsNoTracking().Include(l => l.Product.Category).Include(l => l.Product.Seller)
            .Include(l => l.Product.ProductImages.Where(i => i.IsPrimary))
            .Where(l => l.Product.SellerId == seller.SellerId && l.Product.Status != "archived"
                && l.ListingId == l.Product.Listings.Max(x => x.ListingId));
        var available = Commerce.Available(db).Where(l => l.Product.SellerId != seller.SellerId
            && l.ListingQuantity > 0 && l.Product.Inventory!.AvailableQuantity > 0
            && (l.SellingMethod == "whole_lot"
                ? l.LotUnits <= l.ListingQuantity && l.LotUnits <= l.Product.Inventory.AvailableQuantity
                : l.MinOrderQuantity <= l.ListingQuantity && l.MinOrderQuantity <= l.Product.Inventory.AvailableQuantity));
        if (selectedCategory != 0) available = available.Where(l => l.Product.CategoryId == selectedCategory);
        if (!string.IsNullOrWhiteSpace(q))
        {
            own = own.Where(l => l.Product.ProductName.Contains(q) || l.Product.Category.CategoryName.Contains(q) || l.Product.Seller.StoreName.Contains(q));
            available = available.Where(l => l.Product.ProductName.Contains(q) || l.Product.Category.CategoryName.Contains(q) || l.Product.Seller.StoreName.Contains(q));
        }
        if (offer is not null)
        {
            own = own.Where(l => l.SellingMethod == offer);
            available = available.Where(l => l.SellingMethod == offer);
        }
        var cards = await own.OrderByDescending(l => l.ListingId).ToListAsync();
        cards.AddRange(await available.OrderByDescending(l => l.ListingId).Take(120).ToListAsync());
        if (form is { Id: 0, CategoryId: 0 }) form.CategoryId = selectedCategory;
        if (photo is not null && !SafeImage(photo) && photo != PhotoUrl(form!.Id)) photo = null;
        return View(nameof(Listings), new SellerMarketplace(seller, categories, cards, form, q, selectedCategory,
            offer, storeCategory?.CategoryName, photo));
    }

    public IActionResult Edit(long id = 0) => id == 0
        ? RedirectToAction(nameof(Listings), new { add = true })
        : RedirectToAction(nameof(Listings), new { edit = id });

    private IQueryable<ProductImage> PrimaryImage(long id) => db.ProductImages.Where(i => i.ProductId == id && i.IsPrimary);
    internal static string PhotoUrl(long id) => $"/Market/ProductPhoto/{id}";

    [HttpPost]
    public async Task<IActionResult> Edit(ProductForm form)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == form.CategoryId && c.IsActive))
            ModelState.AddModelError(nameof(form.CategoryId), "Choose an active category.");
        if (!string.IsNullOrWhiteSpace(form.ImageUrl) && !SafeImage(form.ImageUrl))
            ModelState.AddModelError(nameof(form.ImageUrl), "Use an HTTPS image link.");
        if (!form.WholeLot && form.Quantity > 0 && form.Minimum > form.Quantity)
            ModelState.AddModelError(nameof(form.Minimum), "The minimum order cannot exceed available stock.");
        if (form.WholeLot && (form.Quantity < 1 || form.LotUnits > form.Quantity))
            ModelState.AddModelError(nameof(form.Quantity), "Available quantity must cover one complete lot.");
        if (form.Id != 0 && !await Products.AnyAsync(p => p.ProductId == form.Id && p.Status != "archived"))
            return NotFound();
        var (bytes, contentType) = await SellerAnalysisPhoto.ReadAsync(form.Photo, HttpContext.RequestAborted, 5 * 1024 * 1024, allowGif: true);
        if (form.Photo is not null && bytes is null)
            ModelState.AddModelError(nameof(form.Photo), "Choose a PNG, JPEG, WEBP, or GIF image up to 5 MB.");
        if (!ModelState.IsValid)
        {
            var photo = await PrimaryImage(form.Id).Select(i => i.ImageUrl).FirstOrDefaultAsync();
            return await Marketplace(form, null, null, null, photo);
        }

        await using var transaction = await Commerce.Lock(db);
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        var product = form.Id == 0 ? new Product { SellerId = seller.SellerId }
            : await Products.SingleOrDefaultAsync(p => p.ProductId == form.Id && p.Status != "archived");
        if (product is null) return NotFound();
        if (form.Id == 0) db.Products.Add(product);
        product.ProductName = form.Name.Trim();
        product.Description = form.Description;
        product.ProductCondition = form.Condition;
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
        listing.LotUnits = form.WholeLot ? form.LotUnits ?? form.Quantity : null;
        listing.MinOrderQuantity = form.WholeLot ? 1 : form.Minimum;
        listing.ListingQuantity = form.Quantity;

        if (form.Id == 0) await db.SaveChangesAsync();
        var image = await PrimaryImage(product.ProductId).FirstOrDefaultAsync();
        var photoUrl = PhotoUrl(product.ProductId);
        var wasUploaded = image?.ImageUrl == photoUrl;
        var imageUrl = bytes is not null ? photoUrl : form.ImageUrl ?? (wasUploaded ? photoUrl : null);
        if (imageUrl is null && image is not null) db.ProductImages.Remove(image);
        else if (image is not null && imageUrl is not null) image.ImageUrl = imageUrl;
        else if (imageUrl is not null)
            db.ProductImages.Add(new() { Product = product, ImageUrl = imageUrl, IsPrimary = true });

        if (bytes is not null || wasUploaded && imageUrl != photoUrl)
        {
            var storedPhoto = await db.ListingPhotos.FindAsync(product.ProductId);
            if (bytes is not null)
            {
                if (storedPhoto is null) db.ListingPhotos.Add(storedPhoto = new() { ProductId = product.ProductId });
                storedPhoto.Content = bytes;
                storedPhoto.ContentType = contentType!;
            }
            else if (storedPhoto is not null) db.ListingPhotos.Remove(storedPhoto);
        }
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
        return Notice("product-archived", "Seller", nameof(Listings));
    }

    public async Task<IActionResult> Sales() => View(await db.OrderItems.Include(i => i.Order)
        .Where(i => i.Seller.UserId == UserId).OrderByDescending(i => i.Order.CreatedAt).Take(100).ToListAsync());

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
        var seller = await SellerSubscriptions.SellerId(db, UserId);
        ViewBag.PremiumActive = await SellerSubscriptions.HasPremium(db, seller, DateTime.UtcNow, environment.IsDevelopment());
        ViewBag.CanRecommend = await SellerSubscriptions.CanRecommend(db, seller, environment.IsDevelopment());
        return View();
    }

    public IActionResult Insights() => RedirectToAction("Index", "Surplus");

    [HttpPost]
    public IActionResult Analyze(long id) => RedirectToAction("Index", "Surplus", new { productId = id });

    private static Listing? Latest(Product p) => p.Listings.MaxBy(l => l.ListingId);
    internal static bool SafeImage(string url) => (url.StartsWith("/catalog/", StringComparison.Ordinal) && !url.Contains("..") && !url.Contains('\\'))
        || Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "https";
}





