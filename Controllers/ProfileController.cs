using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;

namespace StockMind.Controllers;

[Authorize]
public sealed class ProfileController(StockMindDb db) : StockController
{
    private IQueryable<Models.Database.User> Member => db.Users.Include(u => u.SellerProfile).Include(u => u.UserPreference)
        .Where(u => u.UserId == UserId);

    public async Task<IActionResult> Index()
    {
        var member = await Member.SingleAsync();
        ViewBag.Email = member.Email;
        ViewBag.HasPhoto = await db.RegistrationProfiles.AnyAsync(p => p.UserId == UserId && p.Photo != null);
        return View(new ProfileForm
        {
            FullName = member.FullName, Phone = member.Phone, StoreName = member.SellerProfile?.StoreName,
            City = member.SellerProfile?.City, Description = member.SellerProfile?.Description,
            OrderNotifications = member.UserPreference?.OrderNotifications ?? true,
            ShippingNotifications = member.UserPreference?.ShippingNotifications ?? true,
            MarketingNotifications = member.UserPreference?.MarketingNotifications ?? false
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileForm form)
    {
        await using var transaction = await Commerce.Lock(db);
        var member = await Member.SingleAsync();
        ViewBag.Email = member.Email;
        ViewBag.HasPhoto = await db.RegistrationProfiles.AnyAsync(p => p.UserId == UserId && p.Photo != null);
        if (member.SellerProfile is not null && string.IsNullOrWhiteSpace(form.StoreName))
            ModelState.AddModelError(nameof(form.StoreName), "Enter a store name.");
        if (!ModelState.IsValid) return View(form);
        member.FullName = form.FullName;
        member.Phone = form.Phone;
        member.UpdatedAt = DateTime.UtcNow;
        if (member.SellerProfile is { } seller)
        {
            seller.StoreName = form.StoreName!;
            seller.City = form.City;
            seller.Description = form.Description;
        }
        member.UserPreference ??= new() { UserId = UserId };
        member.UserPreference.OrderNotifications = form.OrderNotifications;
        member.UserPreference.ShippingNotifications = form.ShippingNotifications;
        member.UserPreference.MarketingNotifications = form.MarketingNotifications;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Notice("Profile saved.", "Profile");
    }

    public async Task<IActionResult> Notifications() => View(await db.Notifications.Where(n => n.UserId == UserId)
        .OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Read()
    {
        await db.Notifications.Where(n => n.UserId == UserId && !n.IsRead).ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));
        return RedirectToAction(nameof(Notifications));
    }
}

