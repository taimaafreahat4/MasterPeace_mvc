SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;

IF DB_NAME() <> N'StockMindMVC'
    THROW 50010, 'Run this script against the restored StockMindMVC database.', 1;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = N'Beauty & Personal Care')
    INSERT dbo.Categories (CategoryName, IsActive) VALUES (N'Beauty & Personal Care', 1);

-- Photos and prices come from the supplied storefront files. Stock is sample stock.
DECLARE @catalog TABLE (
    SKU nvarchar(80), CategoryName nvarchar(120), ProductName nvarchar(200),
    Description nvarchar(500), ImageUrl nvarchar(1000), Price decimal(12,2), Minimum int, Quantity int
);
INSERT @catalog VALUES
    (N'SM-ASSET-HEADPHONES', N'Electronics', N'Wired over-ear headphones', N'Blue and black over-ear headphones with a wired connection and inline control.', N'/catalog/headphones.jpg', 25, 5, 30),
    (N'SM-ASSET-PULLOVER', N'Clothing', N'Brown collared knit pullover', N'Brown V-neck knit pullover with a contrasting light collar.', N'/catalog/pullover.webp', 6, 20, 60),
    (N'SM-ASSET-CREAM', N'Beauty & Personal Care', N'Moisturizing face cream, 50 mL', N'Face cream in a 50 mL jar, as shown in the supplied product image.', N'/catalog/face-cream.png', 8, 10, 40);

DECLARE @sellerId bigint = (SELECT MIN(SellerID) FROM dbo.SellerProfiles WHERE VerificationStatus = N'approved');
IF @sellerId IS NULL THROW 50011, 'The restored database has no approved seller.', 1;

DECLARE @newProducts TABLE (ProductId bigint, SKU nvarchar(80));
INSERT dbo.Products (SellerID, CategoryID, ProductName, Description, SKU, ProductCondition, ApprovalStatus, Status, SubmittedAt)
OUTPUT inserted.ProductID, inserted.SKU INTO @newProducts
SELECT @sellerId, category.CategoryID, item.ProductName, item.Description, item.SKU, N'New', N'approved', N'active', SYSUTCDATETIME()
FROM @catalog item
JOIN dbo.Categories category ON category.CategoryName = item.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products product WHERE product.SKU = item.SKU);

INSERT dbo.Inventory (ProductID, AvailableQuantity, ReorderLevel)
SELECT product.ProductId, item.Quantity, item.Minimum
FROM @newProducts product JOIN @catalog item ON item.SKU = product.SKU;

INSERT dbo.ProductImages (ProductID, ImageUrl, IsPrimary)
SELECT product.ProductId, item.ImageUrl, 1
FROM @newProducts product JOIN @catalog item ON item.SKU = product.SKU;

INSERT dbo.Listings (ProductID, SellingMethod, UnitPrice, MinOrderQuantity, ListingQuantity, Status, PublishedAt)
SELECT product.ProductId, N'regular', item.Price, item.Minimum, item.Quantity, N'active', SYSUTCDATETIME()
FROM @newProducts product JOIN @catalog item ON item.SKU = product.SKU;

COMMIT;

