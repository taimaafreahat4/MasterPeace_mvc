using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class StoreProfileForm
{
    [Required, StringLength(80, MinimumLength = 2)] public string StoreName { get; set; } = "";
    [Required, StringLength(60)] public string BusinessCategory { get; set; } = "";
    [Required, StringLength(180, MinimumLength = 4)] public string LocationDetails { get; set; } = "";
    [Required, StringLength(80, MinimumLength = 2)] public string City { get; set; } = "";
    [Required, StringLength(80, MinimumLength = 2)] public string Country { get; set; } = "";
    [Required, StringLength(40)] public string StoreType { get; set; } = "";
    [Required, StringLength(60)] public string TargetCustomers { get; set; } = "";
    [Required, Range(0, 1000000)] public int? AverageVisitorsPerDay { get; set; }
    [StringLength(500)] public string? Description { get; set; }
}
