using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StockMind.Models.Database;

namespace StockMind.Data;

public partial class StockMindDb : DbContext
{
    public StockMindDb(DbContextOptions<StockMindDb> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AdvisorAnalysis> AdvisorAnalyses { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ModerationAction> ModerationActions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlanFeature> PlanFeatures { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<RecommendationAnalysis> RecommendationAnalyses { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SellerProfile> SellerProfiles { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<ShipmentEvent> ShipmentEvents { get; set; }

    public virtual DbSet<SiteFeedback> SiteFeedbacks { get; set; }

    public virtual DbSet<SiteSetting> SiteSettings { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPreference> UserPreferences { get; set; }

    public virtual DbSet<VwHomeFeedback> VwHomeFeedbacks { get; set; }

    public virtual DbSet<VwPublicListing> VwPublicListings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Addresse__091C2A1B7EBB061A");

            entity.HasIndex(e => e.UserId, "UX_Addresses_OneDefault")
                .IsUnique()
                .HasFilter("([IsDefault]=(1))");

            entity.Property(e => e.AddressId).HasColumnName("AddressID");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.PostalCode).HasMaxLength(30);
            entity.Property(e => e.RecipientName).HasMaxLength(150);
            entity.Property(e => e.StreetAddress).HasMaxLength(400);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Addresses__UserI__59063A47");
        });

        modelBuilder.Entity<AdvisorAnalysis>(entity =>
        {
            entity.HasKey(e => e.AnalysisId).HasName("PK__AdvisorA__5B789DE89210F020");

            entity.Property(e => e.AnalysisId).HasColumnName("AnalysisID");
            entity.Property(e => e.BusinessCategory).HasMaxLength(60);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ExpectedSellingPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ProductImageUrl).HasMaxLength(1000);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.PurchaseUnitCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Recommendation).HasMaxLength(30);
            entity.Property(e => e.RiskScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.StoreLocation).HasMaxLength(400);
            entity.Property(e => e.StoreType).HasMaxLength(40);
            entity.Property(e => e.TargetCustomers).HasMaxLength(60);

            entity.HasOne(d => d.Category).WithMany(p => p.AdvisorAnalyses)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__AdvisorAn__Categ__681373AD");

            entity.HasOne(d => d.Seller).WithMany(p => p.AdvisorAnalyses)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AdvisorAn__Selle__671F4F74");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__A17F23B8FAA3E0E9");

            entity.Property(e => e.AuditId).HasColumnName("AuditID");
            entity.Property(e => e.ActionName).HasMaxLength(100);
            entity.Property(e => e.ActorUserId).HasColumnName("ActorUserID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityId)
                .HasMaxLength(80)
                .HasColumnName("EntityID");
            entity.Property(e => e.EntityName).HasMaxLength(100);

            entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActorUserId)
                .HasConstraintName("FK__AuditLogs__Actor__0697FACD");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD79718576978");

            entity.HasIndex(e => e.UserId, "UQ__Carts__1788CCAD8981EFC8").IsUnique();

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carts__UserID__0F624AF8");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2A83CC1883");

            entity.HasIndex(e => new { e.CartId, e.ListingId }, "UQ__CartItem__4A4F3C79896186E5").IsUnique();

            entity.Property(e => e.CartItemId).HasColumnName("CartItemID");
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__CartI__14270015");

            entity.HasOne(d => d.Listing).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Listi__151B244E");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B70831C69");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E07933D652").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(120);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK__Categorie__Paren__656C112C");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAF553D7CA39");

            entity.HasIndex(e => new { e.UserId, e.ListingId }, "UQ__Favorite__0C7B27428F08A9E0").IsUnique();

            entity.Property(e => e.FavoriteId).HasColumnName("FavoriteID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Listing).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__Listi__1CBC4616");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Favorites__UserI__1BC821DD");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__Inventor__F5FDE6D3F04D20E5");

            entity.ToTable("Inventory");

            entity.HasIndex(e => e.ProductId, "UQ__Inventor__B40CC6ECCAFFC298").IsUnique();

            entity.Property(e => e.InventoryId).HasColumnName("InventoryID");
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Product).WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Produ__7A672E12");
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.MovementId).HasName("PK__Inventor__D1822466DFCBE709");

            entity.Property(e => e.MovementId).HasColumnName("MovementID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.InventoryId).HasColumnName("InventoryID");
            entity.Property(e => e.MovementType).HasMaxLength(25);
            entity.Property(e => e.Reason).HasMaxLength(250);
            entity.Property(e => e.ReferenceOrderId).HasColumnName("ReferenceOrderID");

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Inven__30C33EC3");

            entity.HasOne(d => d.ReferenceOrder).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.ReferenceOrderId)
                .HasConstraintName("FK__Inventory__Refer__31B762FC");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__Listings__BF3EBEF07A353070");

            entity.HasIndex(e => new { e.ProductId, e.Status }, "IX_Listings_Product_Status");

            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.LotPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MinOrderQuantity).HasDefaultValue(1);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SellingMethod)
                .HasMaxLength(20)
                .HasDefaultValue("regular");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("draft");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.Listings)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Listings__Produc__02FC7413");
        });

        modelBuilder.Entity<ModerationAction>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("PK__Moderati__FFE3F4B9FC129104");

            entity.Property(e => e.ActionId).HasColumnName("ActionID");
            entity.Property(e => e.ActionType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.ModeratorUserId).HasColumnName("ModeratorUserID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");

            entity.HasOne(d => d.Feedback).WithMany(p => p.ModerationActions)
                .HasForeignKey(d => d.FeedbackId)
                .HasConstraintName("FK__Moderatio__Feedb__16CE6296");

            entity.HasOne(d => d.ModeratorUser).WithMany(p => p.ModerationActions)
                .HasForeignKey(d => d.ModeratorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Moderatio__Moder__13F1F5EB");

            entity.HasOne(d => d.Product).WithMany(p => p.ModerationActions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Moderatio__Produ__14E61A24");

            entity.HasOne(d => d.Review).WithMany(p => p.ModerationActions)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__Moderatio__Revie__15DA3E5D");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E322860848C");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }, "IX_Notifications_User_Read").IsDescending(false, false, true);

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.Body).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.NotificationType).HasMaxLength(40);
            entity.Property(e => e.Title).HasMaxLength(180);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__7755B73D");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFBEDB476F");

            entity.HasIndex(e => new { e.BuyerId, e.CreatedAt }, "IX_Orders_Buyer_Date").IsDescending(false, true);

            entity.HasIndex(e => e.OrderNumber, "UQ__Orders__CAC5E743DA85AEDA").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.BuyerId).HasColumnName("BuyerID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.OrderNumber).HasMaxLength(40);
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(25)
                .HasDefaultValue("pending");
            entity.Property(e => e.ServiceFee).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingFee).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ShippingName).HasMaxLength(150);
            entity.Property(e => e.ShippingPhone).HasMaxLength(30);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Buyer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("FK__Orders__BuyerID__2180FB33");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A1596F3B9B");

            entity.HasIndex(e => e.SellerId, "IX_OrderItems_Seller");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.LineTotal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductNameSnapshot).HasMaxLength(200);
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.SellingMethodSnapshot).HasMaxLength(20);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Listing).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Listi__2B0A656D");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__2A164134");

            entity.HasOne(d => d.Seller).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Selle__2BFE89A6");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58ABF3FAAE");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("JOD")
                .IsFixedLength();
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentMethod).HasMaxLength(40);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(25)
                .HasDefaultValue("pending");
            entity.Property(e => e.ProviderReference).HasMaxLength(180);
            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Payments__OrderI__4B7734FF");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK__Payments__Subscr__4C6B5938");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Payments__UserID__4D5F7D71");
        });

        modelBuilder.Entity<PlanFeature>(entity =>
        {
            entity.HasKey(e => e.FeatureId).HasName("PK__PlanFeat__82230A29349A4767");

            entity.HasIndex(e => e.FeatureName, "UQ__PlanFeat__55ABBB71B718C819").IsUnique();

            entity.Property(e => e.FeatureId).HasColumnName("FeatureID");
            entity.Property(e => e.FeatureName).HasMaxLength(200);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED91E6F96D");

            entity.HasIndex(e => e.CategoryId, "IX_Products_Category");

            entity.HasIndex(e => new { e.SellerId, e.ApprovalStatus }, "IX_Products_Seller_Approval");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(20)
                .HasDefaultValue("pending");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ProductCondition)
                .HasMaxLength(30)
                .HasDefaultValue("New");
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);
            entity.Property(e => e.ReviewedByUserId).HasColumnName("ReviewedByUserID");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.Sku)
                .HasMaxLength(80)
                .HasColumnName("SKU");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Catego__6A30C649");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.Products)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK__Products__Review__6D0D32F4");

            entity.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Seller__693CA210");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F4ECB0687E6A");

            entity.HasIndex(e => e.ProductId, "UX_ProductImages_OnePrimary")
                .IsUnique()
                .HasFilter("([IsPrimary]=(1))");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductIm__Produ__74AE54BC");
        });

        modelBuilder.Entity<RecommendationAnalysis>(entity =>
        {
            entity.HasKey(e => e.RecommendationId).HasName("PK__Recommen__AA15BEC4C907FE6B");

            entity.Property(e => e.RecommendationId).HasColumnName("RecommendationID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.InventoryProductId).HasColumnName("InventoryProductID");
            entity.Property(e => e.ProductImageUrl).HasMaxLength(1000);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.SalesPerWeek).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.SuggestedAction).HasMaxLength(100);
            entity.Property(e => e.SuggestedDiscountPct).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.InventoryProduct).WithMany(p => p.RecommendationAnalyses)
                .HasForeignKey(d => d.InventoryProductId)
                .HasConstraintName("FK__Recommend__Inven__6FB49575");

            entity.HasOne(d => d.Seller).WithMany(p => p.RecommendationAnalyses)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Recommend__Selle__6EC0713C");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AE8466A957");

            entity.HasIndex(e => new { e.ApprovalStatus, e.CreatedAt }, "IX_Reviews_Moderation").IsDescending(false, true);

            entity.HasIndex(e => e.OrderItemId, "UQ__Reviews__57ED06A035A81C4A").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(20)
                .HasDefaultValue("pending");
            entity.Property(e => e.Comment).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);
            entity.Property(e => e.ReviewedByUserId).HasColumnName("ReviewedByUserID");
            entity.Property(e => e.ReviewerId).HasColumnName("ReviewerID");

            entity.HasOne(d => d.OrderItem).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__OrderIt__5E8A0973");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.ReviewReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK__Reviews__Reviewe__6166761E");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__Reviewe__5F7E2DAC");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A6D228176");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616080394F3F").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(30);
        });

        modelBuilder.Entity<SellerProfile>(entity =>
        {
            entity.HasKey(e => e.SellerId).HasName("PK__SellerPr__7FE3DBA1E328B133");

            entity.HasIndex(e => e.UserId, "UQ__SellerPr__1788CCADD9E5CAE5").IsUnique();

            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.BusinessCategory).HasMaxLength(60);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LocationDetails).HasMaxLength(400);
            entity.Property(e => e.LogoUrl).HasMaxLength(1000);
            entity.Property(e => e.StoreName).HasMaxLength(180);
            entity.Property(e => e.StoreType).HasMaxLength(40);
            entity.Property(e => e.TargetCustomers).HasMaxLength(60);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.User).WithOne(p => p.SellerProfile)
                .HasForeignKey<SellerProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SellerPro__UserI__5DCAEF64");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__5CAD378DA30D5544");

            entity.Property(e => e.ShipmentId).HasColumnName("ShipmentID");
            entity.Property(e => e.Carrier).HasMaxLength(100);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.ShipmentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("processing");
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);

            entity.HasOne(d => d.Order).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shipments__Order__55009F39");

            entity.HasOne(d => d.Seller).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK__Shipments__Selle__55F4C372");
        });

        modelBuilder.Entity<ShipmentEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Shipment__7944C870753CD223");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EventDescription).HasMaxLength(400);
            entity.Property(e => e.EventLocation).HasMaxLength(180);
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ShipmentId).HasColumnName("ShipmentID");
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Shipment).WithMany(p => p.ShipmentEvents)
                .HasForeignKey(d => d.ShipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShipmentE__Shipm__59C55456");
        });

        modelBuilder.Entity<SiteFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__SiteFeed__6A4BEDF63C9563C4");

            entity.ToTable("SiteFeedback");

            entity.HasIndex(e => new { e.ApprovalStatus, e.SubmittedAt }, "IX_SiteFeedback_Moderation").IsDescending(false, true);

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(20)
                .HasDefaultValue("pending");
            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.DisplayName).HasMaxLength(150);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);
            entity.Property(e => e.ReviewedByUserId).HasColumnName("ReviewedByUserID");
            entity.Property(e => e.SourceType).HasMaxLength(30);
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Order).WithMany(p => p.SiteFeedbacks)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__SiteFeedb__Order__0C50D423");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.SiteFeedbackReviewedByUsers)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK__SiteFeedb__Revie__0F2D40CE");

            entity.HasOne(d => d.User).WithMany(p => p.SiteFeedbackUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SiteFeedb__UserI__0B5CAFEA");
        });

        modelBuilder.Entity<SiteSetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PK__SiteSett__01E719AC44C982F2");

            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("UpdatedByUserID");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.SiteSettings)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK__SiteSetti__Updat__02C769E9");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Subscrip__9A2B24BDB500E967");

            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.PricePaid).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValue("scheduled");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__PlanI__43D61337");

            entity.HasOne(d => d.Seller).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__Selle__42E1EEFE");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Subscrip__755C22D73A4DA1CE");

            entity.HasIndex(e => e.PlanCode, "UQ__Subscrip__DDC8069B4972707B").IsUnique();

            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("JOD")
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PlanCode).HasMaxLength(30);
            entity.Property(e => e.PlanName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");

            entity.HasMany(d => d.Features).WithMany(p => p.Plans)
                .UsingEntity<Dictionary<string, object>>(
                    "SubscriptionPlanFeature",
                    r => r.HasOne<PlanFeature>().WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Subscript__Featu__40058253"),
                    l => l.HasOne<SubscriptionPlan>().WithMany()
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Subscript__PlanI__3F115E1A"),
                    j =>
                    {
                        j.HasKey("PlanId", "FeatureId");
                        j.ToTable("SubscriptionPlanFeatures");
                        j.IndexerProperty<int>("PlanId").HasColumnName("PlanID");
                        j.IndexerProperty<int>("FeatureId").HasColumnName("FeatureID");
                    });
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__SupportT__712CC6270C379C56");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.AssignedAdminId).HasColumnName("AssignedAdminID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.TicketStatus)
                .HasMaxLength(20)
                .HasDefaultValue("open");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.AssignedAdmin).WithMany(p => p.SupportTicketAssignedAdmins)
                .HasForeignKey(d => d.AssignedAdminId)
                .HasConstraintName("FK__SupportTi__Assig__7E02B4CC");

            entity.HasOne(d => d.User).WithMany(p => p.SupportTicketUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SupportTi__UserI__7C1A6C5A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC91CEB90A");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053437FF7494").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(20)
                .HasDefaultValue("active");
            entity.Property(e => e.AvatarUrl).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleID__5165187F");
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserPref__1788CCAC47D27CBD");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.OrderNotifications).HasDefaultValue(true);
            entity.Property(e => e.ShippingNotifications).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithOne(p => p.UserPreference)
                .HasForeignKey<UserPreference>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserPrefe__UserI__1C873BEC");
        });

        modelBuilder.Entity<VwHomeFeedback>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_HomeFeedback");

            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.DisplayName).HasMaxLength(150);
            entity.Property(e => e.FeedbackId)
                .ValueGeneratedOnAdd()
                .HasColumnName("FeedbackID");
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<VwPublicListing>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PublicListings");

            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.LotPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SellingMethod).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(12, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

