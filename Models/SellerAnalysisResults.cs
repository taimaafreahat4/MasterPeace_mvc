using System.Text.Json;

namespace StockMind.Models;

public sealed record SellerAnalysisSummary(long Id, string Name, string? Action);

public sealed record PurchaseScenario(string? Season, string? SimilarSales, int? SimilarStock, int? MonthlyUnits, int Starting)
{
    public static PurchaseScenario? Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty(nameof(Starting), out _)
                ? document.RootElement.Deserialize<PurchaseScenario>() : null;
        }
        catch (JsonException) { return null; }
    }
}

public sealed record SurplusForecast(int? RecentSales, int HorizonDays, int? Midpoint, int? Low, int? High, string Risk)
{
    public static SurplusForecast? Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty(nameof(HorizonDays), out _)
                ? document.RootElement.Deserialize<SurplusForecast>() : null;
        }
        catch (JsonException) { return null; }
    }
}
