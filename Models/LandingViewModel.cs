using StockMind.Models.Database;

namespace StockMind.Models;

public sealed record CategorySummary(int Id, string Name, int Listings);

public sealed record LandingViewModel(
    IReadOnlyDictionary<string, CategorySummary> Categories,
    IReadOnlyList<VwHomeFeedback> Feedback)
{
    public CategorySummary? Category(string name) => Categories.GetValueOrDefault(name);
}
