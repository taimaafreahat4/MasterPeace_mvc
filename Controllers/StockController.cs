using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace StockMind.Controllers;

public abstract class StockController : Controller
{
    protected long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected IActionResult Notice(string message, string controller, string action = "Index")
    {
        TempData["Notice"] = message;
        return RedirectToAction(action, controller);
    }
}
