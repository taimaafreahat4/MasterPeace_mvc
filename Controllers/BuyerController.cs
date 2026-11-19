using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;

namespace StockMind.Controllers;

[Authorize(Roles = "buyer")]
public sealed class BuyerController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index()
    {
        var name = await db.Users.Where(u => u.UserId == UserId).Select(u => u.FullName).SingleAsync();
        var orders = await db.Orders.AsNoTracking().Include(o => o.OrderItems)
            .Where(o => o.BuyerId == UserId).OrderByDescending(o => o.CreatedAt).ToListAsync();
        return View(new BuyerDashboard(name, orders));
    }
}
