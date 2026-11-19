using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace StockMind.Controllers;

public abstract class StockController : Controller
{
    protected long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected IActionResult Notice(string code, string controller, string action = "Index")
        => RedirectToAction(action, controller, new { notice = code });
}
