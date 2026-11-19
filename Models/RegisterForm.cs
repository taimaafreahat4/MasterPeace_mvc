using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class RegisterForm
{
    [Required, StringLength(90, MinimumLength = 2)] public string FullName { get; set; } = "";
    [Required, StringLength(40, MinimumLength = 3), RegularExpression(@"[A-Za-z0-9._-]+")] public string Username { get; set; } = "";
    [Required, EmailAddress, StringLength(255)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(22)] public string Phone { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 2)] public string Country { get; set; } = "Jordan";
    [Required, StringLength(100, MinimumLength = 2)] public string City { get; set; } = "";
    [Required, StringLength(200, MinimumLength = 6)] public string StreetAddress { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(128, MinimumLength = 12)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    public bool Seller { get; set; }
    public IFormFile? Photo { get; set; }
}
