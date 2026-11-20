using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

public sealed partial class SellerController
{
    public async Task<IActionResult> Plans(long? productId)
    {
        var seller = await SellerSubscriptions.SellerId(db, UserId);
        var free = await db.Set<SellerFreePlan>().FindAsync(seller);
        ViewBag.Demo = environment.IsDevelopment();
        var history = await db.Subscriptions.AsNoTracking().Include(s => s.Plan).Include(s => s.Payments)
            .Where(s => s.SellerId == seller).OrderByDescending(s => s.EndAt).ToListAsync();
        var reports = await db.RecommendationAnalyses.AsNoTracking().Where(r => r.SellerId == seller)
            .OrderByDescending(r => r.RecommendationId)
            .Select(r => new SellerAnalysisSummary(r.RecommendationId, r.ProductName, r.SuggestedAction)).Take(20).ToListAsync();
        return View(new SellerPlans(await SellerSubscriptions.Plans(db).ToListAsync(), history,
            free?.RecommendationUsedAt is not null, productId, reports));
    }

    [HttpPost]
    public async Task<IActionResult> StartFreePlan(long? productId)
    {
        await using var transaction = await Commerce.Lock(db);
        var seller = await SellerSubscriptions.SellerId(db, UserId);
        var free = await db.Set<SellerFreePlan>().FindAsync(seller);
        if (free is null)
        {
            db.Add(new SellerFreePlan { SellerId = seller });
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return free?.RecommendationUsedAt is not null
            ? RedirectToAction(nameof(Plans), new { productId })
            : RedirectToAction("Index", "Surplus", new { productId });
    }

    [HttpGet]
    public async Task<IActionResult> PlanCheckout(int id, long? productId, string? plan)
    {
        var plans = await SellerSubscriptions.Plans(db).ToListAsync();
        var selected = id != 0 ? plans.SingleOrDefault(p => p.PlanId == id)
            : plans.FirstOrDefault(p => p.DurationMonths == (plan switch { "three" => 3, "six" => 6, _ => 1 }));
        if (selected is null) return NotFound();
        var seller = await db.SellerProfiles.AsNoTracking().Include(s => s.User).SingleAsync(s => s.UserId == UserId);
        return ShowCheckout(new SubscriptionCheckoutForm
        {
            CheckoutId = Guid.NewGuid(), PlanId = selected.PlanId, ProductId = productId, FullName = seller.User.FullName,
            Email = seller.User.Email, StoreName = seller.StoreName, Country = seller.Country ?? "Jordan"
        }, plans);
    }

    [HttpPost]
    public async Task<IActionResult> PlanCheckout(SubscriptionCheckoutForm form)
    {
        var plans = await SellerSubscriptions.Plans(db).ToListAsync();
        var plan = plans.SingleOrDefault(p => p.PlanId == form.PlanId);
        if (plan is null) return BadRequest();
        if (form.CheckoutId == Guid.Empty) return BadRequest();
        if (!form.Confirm) ModelState.AddModelError(nameof(form.Confirm), "Confirm the order to continue.");
        if (!ModelState.IsValid) return ShowCheckout(form, plans);

        await using var transaction = await Commerce.Lock(db);
        var seller = await db.SellerProfiles.SingleAsync(s => s.UserId == UserId);
        var existing = await db.Set<SubscriptionCheckout>().AsNoTracking().Include(c => c.Subscription)
            .SingleOrDefaultAsync(c => c.CheckoutId == form.CheckoutId);
        if (existing is not null)
            return existing.Subscription.SellerId == seller.SellerId
                ? RedirectToAction(nameof(PlanConfirmation), new { id = form.CheckoutId, productId = form.ProductId })
                : NotFound();

        var demo = environment.IsDevelopment();
        if (!demo && seller.VerificationStatus != "approved") return Forbid();
        if (await db.Subscriptions.AnyAsync(s => s.SellerId == seller.SellerId && s.Status == "scheduled"))
        {
            ModelState.AddModelError("", "A subscription request is already awaiting confirmation.");
            return ShowCheckout(form, plans);
        }
        var now = DateTime.UtcNow;
        var premium = SellerSubscriptions.Premium(db, seller.SellerId, demo);
        if (await premium.AnyAsync(s => s.StartAt > now))
        {
            ModelState.AddModelError("", "Your next Premium plan is already scheduled.");
            return ShowCheckout(form, plans);
        }

        var start = await premium.Where(s => s.EndAt > now).MaxAsync(s => (DateTime?)s.EndAt) ?? now;
        var subscription = new Subscription
        {
            SellerId = seller.SellerId, PlanId = plan.PlanId, Status = demo ? "active" : "scheduled",
            StartAt = start, EndAt = start.AddMonths(plan.DurationMonths), PricePaid = 0
        };
        db.Add(new SubscriptionCheckout
        {
            CheckoutId = form.CheckoutId, Subscription = subscription, FullName = form.FullName.Trim(),
            Email = form.Email.Trim(), StoreName = form.StoreName?.Trim(), Country = form.Country.Trim(), IsDemo = demo
        });
        db.Payments.Add(new Payment
        {
            Subscription = subscription, UserId = UserId, Amount = demo ? 0 : plan.Price, Currency = plan.Currency,
            PaymentMethod = demo ? "demo" : "manual", PaymentStatus = demo ? "paid" : "pending", PaidAt = demo ? now : null
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(nameof(PlanConfirmation), new { id = form.CheckoutId, productId = form.ProductId });
    }

    [HttpGet]
    public async Task<IActionResult> PlanConfirmation(Guid id, long? productId)
    {
        var checkout = await db.Set<SubscriptionCheckout>().AsNoTracking().Include(c => c.Subscription.Plan)
            .SingleOrDefaultAsync(c => c.CheckoutId == id && c.Subscription.Seller.UserId == UserId);
        if (checkout is null) return NotFound();
        ViewBag.ProductId = productId;
        ViewBag.CanRecommend = await SellerSubscriptions.CanRecommend(db, checkout.Subscription.SellerId, environment.IsDevelopment());
        return View(checkout);
    }

    [HttpPost]
    public IActionResult RequestPlan(int id) => RedirectToAction(nameof(PlanCheckout), new { id });

    private IActionResult ShowCheckout(SubscriptionCheckoutForm form, IReadOnlyList<SubscriptionPlan> plans)
    {
        ViewBag.Plans = plans;
        ViewBag.Demo = environment.IsDevelopment();
        return View("PlanCheckout", form);
    }
}
