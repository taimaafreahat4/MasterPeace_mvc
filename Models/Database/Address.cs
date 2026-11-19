using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Address
{
    public long AddressId { get; set; }

    public long UserId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string? Phone { get; set; }

    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public string StreetAddress { get; set; } = null!;

    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }

    public virtual User User { get; set; } = null!;
}
