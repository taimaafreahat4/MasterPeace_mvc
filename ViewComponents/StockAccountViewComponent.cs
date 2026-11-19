using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;

namespace StockMind.ViewComponents;

public sealed class StockAccountViewComponent(StockMindDb db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!long.TryParse(UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return View((StockAccountMenu?)null);
        var buyer = UserClaimsPrincipal.IsInRole("buyer");
        var menu = await db.Users.AsNoTracking().Where(u => u.UserId == id)
            .Select(u => new StockAccountMenu(u.FullName, u.Email,
                db.Notifications.Count(n => n.UserId == id && !n.IsRead),
                buyer ? db.CartItems.Count(i => i.Cart.UserId == id) : 0,
                db.RegistrationProfiles.Any(p => p.UserId == id && p.Photo != null)))
            .SingleOrDefaultAsync();
        return View(menu);
    }
}

public sealed record StockAccountMenu(string Name, string Email, int Unread, int Cart, bool HasPhoto);
