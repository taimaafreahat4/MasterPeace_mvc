using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class PersonalDetailsForm
{
    [Required, StringLength(80)] public string FullName { get; set; } = "";
    [Required, StringLength(40, MinimumLength = 3), RegularExpression(@"[A-Za-z0-9._-]+")] public string Username { get; set; } = "";
    [Phone, StringLength(25)] public string? Phone { get; set; }
    [StringLength(60)] public string? Country { get; set; }
    [StringLength(60)] public string? City { get; set; }
    [StringLength(200)] public string? StreetAddress { get; set; }
    public IFormFile? Photo { get; set; }
}
