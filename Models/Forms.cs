using System.ComponentModel.DataAnnotations;

namespace StockMind.Models;

public sealed class AccountForm
{
    [StringLength(10)] public string? RequestedRole { get; set; }
    [Required, EmailAddress, StringLength(255)] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(128)] public string Password { get; set; } = "";
}

public sealed class ShippingForm
{
    [StringLength(300)] public string? DeliveryNotes { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Required, Phone, StringLength(30)] public string Phone { get; set; } = "";
    [Required, StringLength(100)] public string Country { get; set; } = "Jordan";
    [Required, StringLength(100)] public string City { get; set; } = "";
    [Required, StringLength(500)] public string Address { get; set; } = "";
}

public sealed class ProductForm
{
    public long Id { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [StringLength(4000)] public string? Description { get; set; }
    public int CategoryId { get; set; }
    [Range(0, 100000)] public decimal Price { get; set; }
    [Range(0, 100000)] public int Quantity { get; set; }
    [Range(0, 100000)] public int ReorderLevel { get; set; }
    [Range(1, 100000)] public int Minimum { get; set; } = 1;

    public bool WholeLot { get; set; }
    [Required, StringLength(30)] public string Condition { get; set; } = "New";
    public IFormFile? Photo { get; set; }
    [Range(1, 100000)] public int? LotUnits { get; set; }
    [StringLength(1000)] public string? ImageUrl { get; set; }
}

public sealed class ProfileForm
{
    [StringLength(180)] public string? StoreName { get; set; }
    [Required, StringLength(150)] public string FullName { get; set; } = "";
    [Phone, StringLength(30)] public string? Phone { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(4000)] public string? Description { get; set; }
    public bool OrderNotifications { get; set; } = true;
    public bool ShippingNotifications { get; set; } = true;
    public bool MarketingNotifications { get; set; }
}

public sealed class FeedbackForm
{
    [Required, StringLength(2000)] public string Message { get; set; } = "";
    [Range(1, 5)] public byte Rating { get; set; } = 5;
    [EmailAddress, StringLength(255)] public string? Email { get; set; }
    [StringLength(150)] public string? Name { get; set; }
}



