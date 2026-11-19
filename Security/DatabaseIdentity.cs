using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models.Database;

namespace StockMind.Security;

internal sealed class DatabaseIdentity(StockMindDb db) : CookieAuthenticationEvents
{
    private const string PasswordRevision = "stockmind:password-revision";

    public static ClaimsPrincipal CreateTicket(User member) => new(new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, member.UserId.ToString()),
        new Claim(PasswordRevision, Fingerprint(member.PasswordHash))
    ], CookieAuthenticationDefaults.AuthenticationScheme));

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var ticket = context.Principal;
        if (long.TryParse(ticket?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            var current = await db.Users.AsNoTracking().Where(u => u.UserId == id && u.AccountStatus == "active")
                .Select(u => new { u.PasswordHash, u.Role.RoleName })
                .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

            if (current is not null && current.RoleName is "buyer" or "seller" or "admin"
                && ticket!.FindFirstValue(PasswordRevision) == Fingerprint(current.PasswordHash))
            {
                // The role is available to authorization for this request and is never written to the cookie.
                var claims = ticket.Claims.Where(c => c.Type != ClaimTypes.Role).Append(new Claim(ClaimTypes.Role, current.RoleName));
                context.ReplacePrincipal(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
                return;
            }
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }

    private static string Fingerprint(string passwordHash) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)));
}
