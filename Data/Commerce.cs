using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StockMind.Models.Database;

namespace StockMind.Data;

internal static class Commerce
{
    public static IQueryable<Listing> Available(StockMindDb db) => db.Listings.AsNoTracking()
        .Include(l => l.Product).ThenInclude(p => p.ProductImages.Where(image => image.IsPrimary))
        .Include(l => l.Product.Category).Include(l => l.Product.Seller)
        .Include(l => l.Product.Inventory)
        .Where(l => l.Status == "active" && l.Product.Status == "active"
            && l.Product.ApprovalStatus == "approved" && l.Product.Category.IsActive
            && l.Product.Seller.VerificationStatus == "approved"
            && l.Product.Seller.User.AccountStatus == "active"
            && (l.ExpiresAt == null || l.ExpiresAt > DateTime.UtcNow));

    public static decimal Price(Listing listing) => listing.SellingMethod == "whole_lot"
        ? listing.LotPrice!.Value : listing.UnitPrice!.Value;

    public static int Minimum(Listing listing) => listing.SellingMethod == "whole_lot" ? 1 : listing.MinOrderQuantity;

    public static int UnitsPerPurchase(Listing listing) => listing.SellingMethod == "whole_lot" ? listing.LotUnits!.Value : 1;

    public static bool InStock(Listing listing, int quantity) =>
        quantity >= Minimum(listing) && (listing.SellingMethod != "whole_lot" || quantity == 1)
        && (long)quantity * UnitsPerPurchase(listing) <= Math.Min(listing.ListingQuantity, listing.Product.Inventory?.AvailableQuantity ?? 0);

    public static async Task<IDbContextTransaction> Lock(StockMindDb db)
    {
        var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.Database.ExecuteSqlRawAsync("""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource=N'StockMind.Commerce', @LockMode=N'Exclusive',
                    @LockOwner=N'Transaction', @LockTimeout=10000;
                IF @result < 0 THROW 50001, 'StockMind is busy. Please retry.', 1;
                """);
            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }
}

