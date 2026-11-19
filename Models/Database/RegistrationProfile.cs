namespace StockMind.Models.Database;

public sealed class RegistrationProfile
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string Username { get; set; } = "";
    public byte[]? Photo { get; set; }
    public string? PhotoType { get; set; }
}
