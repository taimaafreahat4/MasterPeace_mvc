using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

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
        return Notice("profile-saved", "Profile");
    }

    public async Task<IActionResult> Edit()
    {
        var member = await Member.AsNoTracking().SingleAsync();
        var registration = await db.RegistrationProfiles.AsNoTracking().Where(p => p.UserId == UserId)
            .Select(p => new { p.Username, HasPhoto = p.Photo != null }).SingleOrDefaultAsync();
        var address = await db.Addresses.AsNoTracking().SingleOrDefaultAsync(a => a.UserId == UserId && a.IsDefault);
        ViewBag.Email = member.Email;
        ViewBag.HasPhoto = registration?.HasPhoto ?? false;
        return View(new PersonalDetailsForm
        {
            FullName = member.FullName, Username = registration?.Username ?? "", Phone = member.Phone,
            City = address?.City, Country = address?.Country, StreetAddress = address?.StreetAddress
        });
    }

    [HttpPost, RequestSizeLimit(1_700_000)]
    public async Task<IActionResult> Edit(PersonalDetailsForm form, CancellationToken cancellationToken)
    {
        await using var transaction = await Commerce.Lock(db);
        var member = await Member.SingleAsync();
        var registration = await db.RegistrationProfiles.SingleOrDefaultAsync(p => p.UserId == UserId);
        ViewBag.Email = member.Email;
        ViewBag.HasPhoto = registration?.Photo is not null;

        var sections = new[] { form.City, form.Country, form.StreetAddress };
        if (sections.Any(s => !string.IsNullOrWhiteSpace(s)) && sections.Any(string.IsNullOrWhiteSpace))
            ModelState.AddModelError("", "Fill in all address fields to save a delivery address.");
        if (!ModelState.IsValid) return View(form);

        var username = form.Username.ToLowerInvariant();
        if (await db.RegistrationProfiles.AnyAsync(p => p.Username == username && p.UserId != UserId))
        {
            ModelState.AddModelError(nameof(form.Username), "This username is already registered.");
            return View(form);
        }
        byte[]? photo = null;
        string? photoType = null;
        if (form.Photo is { Length: > 0 } upload)
        {
            if (upload.Length <= 1_572_864)
            {
                using var stream = new MemoryStream();
                await upload.CopyToAsync(stream, cancellationToken);
                photo = stream.ToArray();
                photoType = AccountController.ImageType(photo);
            }
            if (photoType is null)
            {
                ModelState.AddModelError(nameof(form.Photo), "Choose a PNG or JPEG photo up to 1.5 MB.");
                return View(form);
            }
        }

        if (registration is null)
        {
            registration = new RegistrationProfile { User = member };
            db.RegistrationProfiles.Add(registration);
        }
        registration.Username = username;
        if (photoType is not null)
        {
            registration.Photo = photo;
            registration.PhotoType = photoType;
        }
        member.FullName = form.FullName.Trim();
        member.Phone = form.Phone;
        member.UpdatedAt = DateTime.UtcNow;
        var address = await db.Addresses.SingleOrDefaultAsync(a => a.UserId == UserId && a.IsDefault);
        if (sections.All(s => !string.IsNullOrWhiteSpace(s)))
        {
            if (address is null)
            {
                address = new Address { UserId = UserId, IsDefault = true };
                db.Addresses.Add(address);
            }
            address.RecipientName = member.FullName;
            address.Phone = member.Phone;
            address.City = form.City!.Trim();
            address.Country = form.Country!.Trim();
            address.StreetAddress = form.StreetAddress!.Trim();
        }
        else if (address is not null) db.Addresses.Remove(address);

        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException e) when (e.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError(nameof(form.Username), "This username is already registered.");
            return View(form);
        }

        await transaction.CommitAsync(cancellationToken);
        return Notice("profile-saved", "Profile", nameof(Edit));
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



