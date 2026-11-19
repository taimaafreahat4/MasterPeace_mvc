using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class Order
{
    public long OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public long? BuyerId { get; set; }

    public string? GuestEmail { get; set; }

    public string ShippingName { get; set; } = null!;

    public string? ShippingPhone { get; set; }

    public string ShippingCountry { get; set; } = null!;

    public string ShippingCity { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal ServiceFee { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal GrandTotal { get; set; }

    public string OrderStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual ICollection<SiteFeedback> SiteFeedbacks { get; set; } = new List<SiteFeedback>();
}
