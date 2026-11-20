using StockMind.Models.Database;

namespace StockMind.Models;

public sealed record SellerMarketplace(SellerProfile Seller, IReadOnlyList<Category> Categories,
    IReadOnlyList<Listing> Listings, ProductForm? Form, string? Query, int Category, string? Offer,
    string? StoreCategory, string? CurrentPhoto);

public sealed class ListingPhoto
{
    public long ProductId { get; set; }
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = "";
}
