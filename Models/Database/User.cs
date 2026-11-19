using System;
using System.Collections.Generic;

namespace StockMind.Models.Database;

public partial class User
{
    public long UserId { get; set; }

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string AccountStatus { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<ModerationAction> ModerationActions { get; set; } = new List<ModerationAction>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Review> ReviewReviewedByUsers { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewReviewers { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;

    public virtual SellerProfile? SellerProfile { get; set; }

    public virtual ICollection<SiteFeedback> SiteFeedbackReviewedByUsers { get; set; } = new List<SiteFeedback>();

    public virtual ICollection<SiteFeedback> SiteFeedbackUsers { get; set; } = new List<SiteFeedback>();

    public virtual ICollection<SiteSetting> SiteSettings { get; set; } = new List<SiteSetting>();

    public virtual ICollection<SupportTicket> SupportTicketAssignedAdmins { get; set; } = new List<SupportTicket>();

    public virtual ICollection<SupportTicket> SupportTicketUsers { get; set; } = new List<SupportTicket>();

    public virtual UserPreference? UserPreference { get; set; }
}

