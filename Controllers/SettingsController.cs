using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

[Authorize]
public sealed class SettingsController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index()
    {
        var preferences = await db.UserPreferences.AsNoTracking().SingleOrDefaultAsync(p => p.UserId == UserId);
        return View(new PreferencesForm
        {
            OrderNotifications = preferences?.OrderNotifications ?? true,
            ShippingNotifications = preferences?.ShippingNotifications ?? true,
            MarketingNotifications = preferences?.MarketingNotifications ?? false
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(PreferencesForm form)
    {
        if (!ModelState.IsValid) return View(form);
        await using var transaction = await Commerce.Lock(db);
        var preferences = await db.UserPreferences.SingleOrDefaultAsync(p => p.UserId == UserId);
        if (preferences is null)
        {
            preferences = new UserPreference { UserId = UserId };
            db.UserPreferences.Add(preferences);
        }
        preferences.OrderNotifications = form.OrderNotifications;
        preferences.ShippingNotifications = form.ShippingNotifications;
        preferences.MarketingNotifications = form.MarketingNotifications;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Notice("settings-saved", "Settings");
    }
}
