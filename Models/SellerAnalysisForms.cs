using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class SurplusForm
{
    [Required, StringLength(200)] public string Name { get; set; } = "";
    public int CategoryId { get; set; }
    [Range(1, 1000000)] public int Quantity { get; set; } = 1;
    [Range(0.01, 100000)] public decimal Price { get; set; }
    [Range(0, 1000000)] public int? RecentSales { get; set; }
    [Range(0, 3650)] public int AgeDays { get; set; }
    [Range(7, 90)] public int HorizonDays { get; set; } = 30;
    [Required, RegularExpression("New|Good|Fair")] public string Condition { get; set; } = "New";
    public IFormFile? Photo { get; set; }
}
