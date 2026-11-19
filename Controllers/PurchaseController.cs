using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;

namespace StockMind.Controllers;

[Authorize(Roles = "seller")]
public sealed class PurchaseController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index() => await Show(new());

    [HttpPost]
    public async Task<IActionResult> Index(PurchaseForm form)
    {
        if (!await db.Categories.AnyAsync(c => c.CategoryId == form.CategoryId && c.IsActive))
            ModelState.AddModelError(nameof(form.CategoryId), "Choose an active category.");
        if (!ModelState.IsValid) return await Show(form);
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        var margin = (form.SellingPrice - form.UnitCost) / form.SellingPrice;
        var recommendation = margin <= 0 ? "Revise cost or selling price"
            : margin < .15m ? "Review expenses" : "Check recent demand";
        db.AdvisorAnalyses.Add(new()
        {
            SellerId = seller.SellerId,
            ProductName = form.ProductName,
            CategoryId = form.CategoryId,
            PurchaseQuantity = form.Quantity,
            PurchaseUnitCost = form.UnitCost,
            ExpectedSellingPrice = form.SellingPrice,
            Recommendation = recommendation,
            AnalysisJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                PurchaseTotal = form.UnitCost * form.Quantity,
                GrossMarginPercent = Math.Round(margin * 100, 2)
            })
        });
        await db.SaveChangesAsync();
        return Notice("analysis-saved", "Seller", "Insights");
    }

    private async Task<IActionResult> Show(PurchaseForm form)
    {
        ViewBag.Categories = await db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        return View("Index", form);
    }
}


