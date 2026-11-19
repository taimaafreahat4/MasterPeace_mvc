namespace StockMind.Models;

public sealed class PreferencesForm
{
    public bool OrderNotifications { get; set; } = true;
    public bool ShippingNotifications { get; set; } = true;
    public bool MarketingNotifications { get; set; }
}
