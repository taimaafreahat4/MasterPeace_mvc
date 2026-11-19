using System.Net.Mail;
using StockMind.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

public sealed class AccountController(StockMindDb db, IPasswordHasher<User> hasher) : StockController
{
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new AccountForm());
    }

    [HttpPost, EnableRateLimiting("account")]
    public async Task<IActionResult> Login(AccountForm form, string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(form);
        var email = form.Email.Trim().ToLowerInvariant();
        var member = await db.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Email == email);
        if (member is null || member.AccountStatus != "active"
            || !member.PasswordHash.StartsWith("AQAAAA", StringComparison.Ordinal)
            || hasher.VerifyHashedPassword(member, member.PasswordHash, form.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "The email or password is invalid.");
            return View(form);
        }

        await SignIn(member);
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index",
            member.Role.RoleName switch { "admin" => "Admin", "seller" => "Seller", _ => "Market" });
    }

    public IActionResult Register(bool seller = false) => View(new RegisterForm { Seller = seller });

    [HttpPost, EnableRateLimiting("account"), RequestSizeLimit(2_200_000)]
    public async Task<IActionResult> Register(RegisterForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(form);
        var email = form.Email.Trim().ToLowerInvariant();
        var username = form.Username.ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken)
            || await db.RegistrationProfiles.AnyAsync(p => p.Username == username, cancellationToken))
        {
            ModelState.AddModelError("", "The email or username is already registered.");
            return View(form);
        }

        byte[]? photo = null;
        string? photoType = null;
        if (form.Photo is { Length: > 0 } upload)
        {
            if (upload.Length <= 2_097_152)
            {
                using var stream = new MemoryStream();
                await upload.CopyToAsync(stream, cancellationToken);
                photo = stream.ToArray();
                photoType = ImageType(photo);
            }
            if (photoType is null)
            {
                ModelState.AddModelError(nameof(form.Photo), "Choose a PNG or JPEG photo up to 2 MB.");
                return View(form);
            }
        }

        var role = await db.Roles.SingleAsync(r => r.RoleName == (form.Seller ? "seller" : "buyer"), cancellationToken);
        var member = Create(hasher, role, email, form.FullName.Trim(), form.Password);
        member.Phone = form.Phone.Trim();
        member.Addresses.Add(new Address
        {
            RecipientName = member.FullName, Phone = member.Phone, Country = form.Country.Trim(),
            City = form.City.Trim(), StreetAddress = form.StreetAddress.Trim(), IsDefault = true
        });
        if (form.Seller) member.SellerProfile = new SellerProfile
        {
            StoreName = member.FullName, Country = form.Country.Trim(), City = form.City.Trim()
        };
        db.RegistrationProfiles.Add(new RegistrationProfile
        {
            User = member, Username = username, Photo = photo, PhotoType = photoType
        });
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException e) when (e.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError("", "The email or username is already registered.");
            return View(form);
        }
        await SignIn(member);
        return RedirectToAction("Index", form.Seller ? "Seller" : "Market");
    }

    [Authorize]
    public async Task<IActionResult> Photo(CancellationToken cancellationToken)
    {
        var image = await db.RegistrationProfiles.Where(p => p.UserId == UserId)
            .Select(p => new { p.Photo, p.PhotoType }).SingleOrDefaultAsync(cancellationToken);
        if (image?.Photo is null || image.PhotoType is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(image.Photo, image.PhotoType);
    }

    private static string? ImageType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (bytes.Length >= 8 && bytes.StartsWith(new byte[] { 255, 216, 255 })) return "image/jpeg";
        return null;
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => StatusCode(403);

    private Task SignIn(User member) => HttpContext.SignInAsync(DatabaseIdentity.CreateTicket(member));

    internal static async Task ResetPassword(IServiceProvider services, string email)
    {
        var password = Environment.GetEnvironmentVariable("STOCKMIND_PROVISION_PASSWORD");
        if (password is null || password.Length is < 12 or > 128)
            throw new ArgumentException("Set STOCKMIND_PROVISION_PASSWORD (12–128 characters).");
        var canonicalEmail = email.Trim().ToLowerInvariant();
        var db = services.GetRequiredService<StockMindDb>();
        var member = await db.Users.SingleOrDefaultAsync(u => u.Email == canonicalEmail)
            ?? throw new InvalidOperationException("No account matches this email.");
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        member.PasswordHash = hasher.HashPassword(member, password);
        member.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        Console.WriteLine("Password updated.");
    }

    private static User Create(IPasswordHasher<User> hasher, Role role, string email, string name, string password)
    {
        var member = new User { Role = role, Email = email, FullName = name, AccountStatus = "active" };
        member.PasswordHash = hasher.HashPassword(member, password);
        return member;
    }

    internal static async Task Provision(IServiceProvider services, string roleName, string email, string name)
    {
        var password = Environment.GetEnvironmentVariable("STOCKMIND_PROVISION_PASSWORD");
        email = email.Trim().ToLowerInvariant();
        if (password is null || password.Length is < 12 or > 128 || email.Length > 255
            || !MailAddress.TryCreate(email, out var address) || address.Address != email
            || string.IsNullOrWhiteSpace(name) || name.Length > 150 || roleName is not ("admin" or "buyer" or "seller"))
            throw new ArgumentException("Supply a valid role, email and name, and set STOCKMIND_PROVISION_PASSWORD (12–128 characters).");
        var db = services.GetRequiredService<StockMindDb>();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        var role = await db.Roles.SingleAsync(r => r.RoleName == roleName);
        if (await db.Users.AnyAsync(u => u.Email == email)) throw new InvalidOperationException("This email already exists.");
        var member = Create(hasher, role, email, name, password);
        if (roleName == "seller") member.SellerProfile = new SellerProfile { StoreName = name };
        db.Users.Add(member);
        await db.SaveChangesAsync();
        Console.WriteLine("Account created.");
    }
}
