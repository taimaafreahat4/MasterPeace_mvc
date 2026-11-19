using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockMind.Data;
using StockMind.Models;
using StockMind.Models.Database;

namespace StockMind.Controllers;

[Authorize(Roles = "buyer")]
public sealed class CartController(StockMindDb db) : StockController
{
    private IQueryable<CartItem> Items => db.CartItems.Where(i => i.Cart.UserId == UserId)
        .Include(i => i.Listing.Product.Inventory)
        .Include(i => i.Listing.Product.Category).Include(i => i.Listing.Product.Seller)
        .Include(i => i.Listing.Product.ProductImages.Where(image => image.IsPrimary));

    public async Task<IActionResult> Index() => View(await Items.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Set(long id, int quantity, bool add = false)
    {
        if (!ModelState.IsValid || quantity < 0 || quantity > 100000) return BadRequest();
        await using var transaction = await Commerce.Lock(db);
        var item = await Items.SingleOrDefaultAsync(i => i.ListingId == id);
        if (quantity == 0)
        {
            if (item is not null) db.CartItems.Remove(item);
        }
        else
        {
            var listing = await Commerce.Available(db).SingleOrDefaultAsync(l => l.ListingId == id);
            var desired = quantity + (add ? item?.Quantity ?? 0 : 0);
            if (listing is null || !Commerce.InStock(listing, desired))
                return Notice("quantity-unavailable", "Cart");

            if (item is null)
            {
                var cart = await db.Carts.SingleOrDefaultAsync(c => c.UserId == UserId);
                if (cart is null)
                {
                    cart = new Cart { UserId = UserId };
                    db.Carts.Add(cart);
                }
                db.CartItems.Add(new CartItem { Cart = cart, ListingId = id, Quantity = desired });
                cart.UpdatedAt = DateTime.UtcNow;
            }
            else item.Quantity = desired;
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Checkout()
    {
        var items = await Items.ToListAsync();
        if (items.Count == 0) return RedirectToAction(nameof(Index));
        await ShowCheckoutSummary(items);
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.UserId == UserId && a.IsDefault);
        var member = await db.Users.Where(u => u.UserId == UserId).Select(u => new { u.FullName, u.Phone }).SingleAsync();
        return View(address is null ? new ShippingForm { Name = member.FullName, Phone = member.Phone ?? "" } : new ShippingForm
        {
            Name = address.RecipientName, Phone = address.Phone ?? "", Country = address.Country,
            City = address.City, Address = address.StreetAddress
        });
    }

    private async Task ShowCheckoutSummary(IReadOnlyList<CartItem> items)
    {
        ViewBag.Items = items;
        ViewBag.BuyerEmail = await db.Users.Where(u => u.UserId == UserId).Select(u => u.Email).SingleAsync();
    }
    [HttpPost]
    public async Task<IActionResult> Checkout(ShippingForm form)
    {
        if (!ModelState.IsValid)
        {
            await ShowCheckoutSummary(await Items.ToListAsync());
            return View(form);
        }
        await using var transaction = await Commerce.Lock(db);
        var items = await Items.OrderBy(i => i.ListingId).ToListAsync();
        if (items.Count == 0) return RedirectToAction(nameof(Index));

        var ids = items.Select(i => i.ListingId).ToArray();
        var available = await Commerce.Available(db).Where(l => ids.Contains(l.ListingId))
            .Select(l => l.ListingId).ToListAsync();
        var order = new Order
        {
            BuyerId = UserId, OrderNumber = "SM-" + Guid.NewGuid().ToString("N"),
            ShippingName = form.Name, ShippingPhone = form.Phone, ShippingCountry = form.Country,
            ShippingCity = form.City, ShippingAddress = form.Address, DeliveryNotes = form.DeliveryNotes?.Trim(), OrderStatus = "processing"
        };

        foreach (var item in items)
        {
            var listing = item.Listing;
            if (!available.Contains(item.ListingId) || !Commerce.InStock(listing, item.Quantity))
                return Notice("stock-changed", "Cart");

            var inventory = listing.Product.Inventory!;
            var units = item.Quantity * Commerce.UnitsPerPurchase(listing);
            inventory.AvailableQuantity -= units;
            inventory.LastUpdated = DateTime.UtcNow;
            listing.ListingQuantity -= units;
            if (listing.ListingQuantity == 0) listing.Status = "sold";
            var price = Commerce.Price(listing);
            var total = price * item.Quantity;
            order.Subtotal += total;
            order.OrderItems.Add(new OrderItem
            {
                ListingId = listing.ListingId, SellerId = listing.Product.SellerId,
                ProductNameSnapshot = listing.Product.ProductName, SellingMethodSnapshot = listing.SellingMethod,
                Quantity = item.Quantity, UnitsPerLot = listing.SellingMethod == "whole_lot" ? listing.LotUnits : null, UnitPrice = price, LineTotal = total
            });
            db.InventoryMovements.Add(new InventoryMovement { Inventory = inventory, ReferenceOrder = order,
                QuantityChange = -units, MovementType = "sale", Reason = "Marketplace checkout" });
        }

        order.ServiceFee = Commerce.ServiceFee(order.Subtotal);
        order.GrandTotal = order.Subtotal + order.ServiceFee;
        if (order.GrandTotal > 9_999_999_999.99m)
            return Notice("order-total-limit", "Cart");

        order.Payments.Add(new Payment { UserId = UserId, Amount = order.GrandTotal,
            Currency = "JOD", PaymentMethod = "cash_on_delivery", PaymentStatus = "pending" });
        foreach (var seller in order.OrderItems.Select(i => i.SellerId).Distinct())
            order.Shipments.Add(new Shipment { SellerId = seller, ShipmentStatus = "processing",
                ShipmentEvents = [new ShipmentEvent { Status = "processing", EventDescription = "Order received" }] });
        if (!await db.UserPreferences.AnyAsync(p => p.UserId == UserId && !p.OrderNotifications))
            db.Notifications.Add(new() { UserId = UserId, Title = "Order received",
                Body = $"Order {order.OrderNumber} is being processed.", NotificationType = "order" });
        db.Orders.Add(order);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return RedirectToAction("Confirmation", "Orders", new { id = order.OrderId });
    }
}





