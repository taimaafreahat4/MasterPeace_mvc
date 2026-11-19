using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;

namespace StockMind.Controllers;

public sealed class HomeController(StockMindDb db) : StockController
{
    public async Task<IActionResult> Index()
    {
        var available = Commerce.Available(db).Where(l => l.ListingQuantity > 0);
        var counts = await available.GroupBy(l => l.Product.CategoryId)
            .Select(group => new { Id = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Id, item => item.Count);
        var categories = (await db.Categories.Where(c => c.IsActive).ToListAsync())
            .ToDictionary(c => c.CategoryName, c => new CategorySummary(c.CategoryId, c.CategoryName,
                counts.GetValueOrDefault(c.CategoryId)), StringComparer.OrdinalIgnoreCase);
        var feedback = await db.VwHomeFeedbacks.OrderByDescending(f => f.SubmittedAt).Take(12).ToListAsync();
        return View(new LandingViewModel(categories, feedback));
    }

    public IActionResult About() => View();
    public IActionResult Policies() => View();
    public IActionResult Error() => View("Status", 500);
    public IActionResult Status(int code) => View(code);

    public IActionResult Feedback() => View(new FeedbackForm());

    [HttpPost]
    public async Task<IActionResult> Feedback(FeedbackForm form)
    {
        if (!ModelState.IsValid) return View(form);
        db.SiteFeedbacks.Add(new()
        {
            UserId = User.Identity?.IsAuthenticated == true ? UserId : null,
            SourceType = "site", Message = form.Message, Rating = form.Rating,
            DisplayName = form.Name, ContactEmail = form.Email, ApprovalStatus = "pending"
        });
        await db.SaveChangesAsync();
        return Notice("feedback-submitted", "Home");
    }

    public IActionResult Help() => View();

    [HttpPost]
    public async Task<IActionResult> Help(string? email, string? subject, string? message)
    {
        var signedIn = User.Identity?.IsAuthenticated == true;
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > 200
            || string.IsNullOrWhiteSpace(message) || message.Length > 4000
            || (!signedIn && (email is null || email.Length > 255 || !System.Net.Mail.MailAddress.TryCreate(email, out _))))
        {
            ModelState.AddModelError("", "Enter a valid subject, message and email.");
            return View();
        }
        db.SupportTickets.Add(new() { UserId = signedIn ? UserId : null, GuestEmail = signedIn ? null : email,
            Subject = subject, Message = message, TicketStatus = "open" });
        await db.SaveChangesAsync();
        return Notice("ticket-submitted", "Home");
    }
}
