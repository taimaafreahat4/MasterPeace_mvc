using StockMind.Security;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<StockMindDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockMind")));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<DatabaseIdentity>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.Cookie.Name = "StockMind.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = false;
    options.EventsType = typeof(DatabaseIdentity);
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("account", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();
if (args is ["provision", var role, var email, var name])
{
    await using var scope = app.Services.CreateAsyncScope();
    await StockMind.Controllers.AccountController.Provision(scope.ServiceProvider, role, email, name);
    return;
}
if (args is ["reset-password", var existingEmail])
{
    await using var scope = app.Services.CreateAsyncScope();
    await StockMind.Controllers.AccountController.ResetPassword(scope.ServiceProvider, existingEmail);
    return;
}
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
var originalRoutes = new Dictionary<string, string>
{
    ["/home/index.html"] = "/",
    ["/home/about.html"] = "/Home/About",
    ["/home/feedback.html"] = "/Home/Feedback",
    ["/home/help.html"] = "/Home/Help",
    ["/home/policies.html"] = "/Home/Policies",
    ["/login/login.html"] = "/Account/Login",
    ["/login/signup.html"] = "/Account/Register",
    ["/buyer-dashboard/dashboard.html"] = "/Buyer",
    ["/category/category.html"] = "/Market",
    ["/favorite/favorite.html"] = "/Market/Favorites",
    ["/cart/cart.html"] = "/Cart",
    ["/checkout/checkout.html"] = "/Cart/Checkout",
    ["/profile/profile.html"] = "/Profile/Edit",
    ["/settings/settings.html"] = "/Settings",
    ["/admin/dashboard/dashboard.html"] = "/Admin",
    ["/admin/products/products.html"] = "/Admin/Products",
    ["/admin/users/users.html"] = "/Admin/Users",
    ["/admin/sellers/sellers.html"] = "/Admin/Sellers",
    ["/admin/orders/orders.html"] = "/Orders",
    ["/admin/settings/settings.html"] = "/Admin/Settings",
    ["/admin/profile/profile.html"] = "/Profile",
    ["/seller/stockmind_seller_center/dashboard.html"] = "/Seller",
    ["/seller/stockmind_seller_center/inventory.html"] = "/Seller/Listings",
    ["/seller/stockmind_seller_center/marketplace.html"] = "/Seller/Listings",
    ["/seller/stockmind_seller_center/add-product.html"] = "/Seller/Edit",
    ["/seller/stockmind_seller_center/ai-tools.html"] = "/Seller/Tools",
    ["/seller/stockmind_seller_center/ai-advisor.html"] = "/Purchase",
    ["/seller/stockmind_seller_center/recommendations.html"] = "/Surplus",
    ["/seller/stockmind_seller_center/subscription.html"] = "/Seller/Plans",
    ["/seller/stockmind_seller_center/subscription-payment.html"] = "/Seller/PlanCheckout",
    ["/seller/stockmind_seller_center/subscription-confirmation.html"] = "/Seller/Plans",
    ["/seller/stockmind_seller_center/store-profile.html"] = "/Seller/StoreProfile"
};
foreach (var (original, destination) in originalRoutes)
    app.MapGet(original, (HttpRequest request) => Results.Redirect(destination + request.QueryString));

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();







