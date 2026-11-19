using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class SupportTicket
{
    public long TicketId { get; set; }

    public long? UserId { get; set; }

    public string? GuestEmail { get; set; }

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string TicketStatus { get; set; } = null!;

    public long? AssignedAdminId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? AssignedAdmin { get; set; }

    public virtual User? User { get; set; }
}
