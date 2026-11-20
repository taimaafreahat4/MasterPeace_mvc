using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

[Authorize(Roles = "seller")]
public sealed class PurchaseController(StockMindDb db) : StockController
{
    private IQueryable<AdvisorAnalysis> Mine => db.AdvisorAnalyses.AsNoTracking().Where(a => a.Seller.UserId == UserId);

    public async Task<IActionResult> Index(long? analysisId)
    {
        if (analysisId is long id)
        {
            var row = await Mine.SingleOrDefaultAsync(a => a.AnalysisId == id);
            if (row is null) return NotFound();
            var scenario = PurchaseScenario.Read(row.AnalysisJson);
            return await Show(new PurchaseForm
            {
                ProductName = row.ProductName, CategoryId = row.CategoryId ?? 0, Quantity = row.PurchaseQuantity ?? 1,
                UnitCost = row.PurchaseUnitCost ?? 0, SellingPrice = row.ExpectedSellingPrice ?? 0,
                StoreLocation = row.StoreLocation, StoreType = row.StoreType, TargetCustomers = row.TargetCustomers,
                VisitorsPerDay = row.VisitorsPerDay, Season = scenario?.Season, SimilarSales = scenario?.SimilarSales,
                SimilarStock = scenario?.SimilarStock
            });
        }

        var store = await db.SellerProfiles.AsNoTracking().SingleAsync(s => s.UserId == UserId);
        return await Show(new PurchaseForm
        {
            StoreLocation = string.Join(", ", new[] { store.LocationDetails, store.City, store.Country }.Where(s => !string.IsNullOrWhiteSpace(s))),
            StoreType = store.StoreType, TargetCustomers = store.TargetCustomers, VisitorsPerDay = store.AverageVisitorsPerDay
        });
    }

    [HttpPost, RequestSizeLimit(1_700_000)]
    public async Task<IActionResult> Index(PurchaseForm form, CancellationToken cancellationToken)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == form.CategoryId && c.IsActive, cancellationToken))
            ModelState.AddModelError(nameof(form.CategoryId), "Choose an active category.");
        var (photo, type) = await SellerAnalysisPhoto.ReadAsync(form.Photo, cancellationToken);
        if (form.Photo is not null && type is null)
            ModelState.AddModelError(nameof(form.Photo), "Choose a JPG, PNG or WEBP image up to 1.5 MB.");
        if (!ModelState.IsValid) return await Show(form);

        var sellerId = await db.SellerProfiles.Where(s => s.UserId == UserId).Select(s => s.SellerId).SingleAsync(cancellationToken);
        var margin = (form.SellingPrice - form.UnitCost) / form.SellingPrice;
        var estimatedUnits = form.VisitorsPerDay is int visits ? (int)Math.Round(visits * 30 * .04m) : (int?)null;
        var starting = margin <= 0 ? 0 : Math.Max(0, Math.Min(form.Quantity, estimatedUnits ?? form.Quantity / 2) - (form.SimilarStock ?? 0));
        var row = new AdvisorAnalysis
        {
            SellerId = sellerId, ProductName = form.ProductName.Trim(), CategoryId = form.CategoryId,
            PurchaseQuantity = form.Quantity, PurchaseUnitCost = form.UnitCost, ExpectedSellingPrice = form.SellingPrice,
            StoreLocation = form.StoreLocation, StoreType = form.StoreType, TargetCustomers = form.TargetCustomers,
            VisitorsPerDay = form.VisitorsPerDay, Photo = photo, PhotoType = type,
            Recommendation = margin <= 0 ? "Revise cost or selling price" : starting == 0 ? "Review existing stock" : "Start with a trial order",
            RiskScore = margin <= 0 ? 95 : margin < .15m ? 70 : estimatedUnits is null || estimatedUnits < form.Quantity ? 50 : 30,
            AnalysisJson = JsonSerializer.Serialize(new PurchaseScenario(form.Season, form.SimilarSales, form.SimilarStock, estimatedUnits, starting))
        };
        db.AdvisorAnalyses.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Report), new { id = row.AnalysisId });
    }

    public async Task<IActionResult> Report(long id)
    {
        var row = await Mine.Include(a => a.Category).SingleOrDefaultAsync(a => a.AnalysisId == id);
        if (row is null) return NotFound();
        ViewBag.Scenario = PurchaseScenario.Read(row.AnalysisJson);
        return View(row);
    }

    public async Task<IActionResult> Download(long id)
    {
        var row = await Mine.SingleOrDefaultAsync(a => a.AnalysisId == id);
        if (row is null) return NotFound();
        var scenario = PurchaseScenario.Read(row.AnalysisJson);
        var report = $"Stock Mind purchase analysis\n{row.ProductName}\nPlanned quantity: {row.PurchaseQuantity}\nUnit cost: {row.PurchaseUnitCost:N2} JOD\nExpected unit price: {row.ExpectedSellingPrice:N2} JOD\nSuggested starting quantity: {scenario?.Starting}\nRecommendation: {row.Recommendation}\nIllustrative estimate based on seller inputs.";
        return File(Encoding.UTF8.GetBytes(report), "text/plain; charset=utf-8", $"stockmind-purchase-{id}.txt");
    }

    public async Task<IActionResult> Photo(long id)
    {
        var image = await Mine.Where(a => a.AnalysisId == id).Select(a => new { a.Photo, a.PhotoType }).SingleOrDefaultAsync();
        if (image?.Photo is null || image.PhotoType is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(image.Photo, image.PhotoType);
    }

    private async Task<IActionResult> Show(PurchaseForm form)
    {
        ViewBag.Categories = await db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.History = await Mine.OrderByDescending(a => a.AnalysisId)
            .Select(a => new SellerAnalysisSummary(a.AnalysisId, a.ProductName, a.Recommendation)).Take(20).ToListAsync();
        return View("Index", form);
    }
}

