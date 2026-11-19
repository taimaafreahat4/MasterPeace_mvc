SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

IF DB_NAME() NOT IN (N'StockMindMVC', N'StockMindMVC_Test')
    THROW 50010, 'Use the StockMind MVC database.', 1;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WITH (UPDLOCK, HOLDLOCK) WHERE CategoryName = N'Beauty & Personal Care')
    INSERT dbo.Categories (CategoryName, IsActive) VALUES (N'Beauty & Personal Care', 1);

DECLARE @seller bigint = (
    SELECT MIN(s.SellerID) FROM dbo.SellerProfiles s JOIN dbo.Users u ON u.UserID = s.UserID
    WHERE s.VerificationStatus = N'approved' AND u.AccountStatus = N'active'
);
IF @seller IS NULL THROW 50011, 'An active approved seller is required.', 1;

DECLARE @catalog TABLE (
    SKU nvarchar(80), CategoryName nvarchar(120), ProductName nvarchar(200),
    Description nvarchar(500), ImageUrl nvarchar(1000), Price decimal(12,2), Quantity int, LotUnits int NULL
);
INSERT @catalog VALUES
    (N'SM-DEMO-HEADPHONES-LOT', N'Electronics', N'Wired headphones (10-unit case)', N'Demo offer for ten wired over-ear headphones, sold as one case.', N'/catalog/headphones.jpg', 190.00, 10, 10),
    (N'SM-DEMO-FACECREAM-LOT', N'Beauty & Personal Care', N'Moisturizing face cream (12-unit case)', N'Demo wholesale case of twelve 50 mL jars.', N'/catalog/face-cream.png', 78.00, 12, 12),
    (N'SM-DEMO-PULLOVER-LOT', N'Clothing', N'Collared knit pullovers (8-unit lot)', N'Demo lot of eight brown collared pullovers.', N'/catalog/pullover.webp', 42.00, 8, 8),
    (N'SM-DEMO-STRIPED-SHORTS', N'Clothing', N'Cream striped tailored shorts', N'Demo listing for cream tailored shorts with vertical stripes.', N'/catalog/striped-shorts.webp', 13.50, 18, NULL);

DECLARE @created TABLE (ProductID bigint, SKU nvarchar(80));
INSERT dbo.Products (SellerID, CategoryID, ProductName, Description, SKU, ProductCondition, ApprovalStatus, Status, SubmittedAt)
OUTPUT inserted.ProductID, inserted.SKU INTO @created
SELECT @seller, c.CategoryID, x.ProductName, x.Description, x.SKU, N'New', N'approved', N'active', SYSUTCDATETIME()
FROM @catalog x JOIN dbo.Categories c ON c.CategoryName = x.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products WITH (UPDLOCK, HOLDLOCK) WHERE SKU = x.SKU);

INSERT dbo.Inventory (ProductID, AvailableQuantity, ReorderLevel)
SELECT p.ProductID, x.Quantity, 0 FROM @created p JOIN @catalog x ON x.SKU = p.SKU;

INSERT dbo.ProductImages (ProductID, ImageUrl, IsPrimary)
SELECT p.ProductID, x.ImageUrl, 1 FROM @created p JOIN @catalog x ON x.SKU = p.SKU;

INSERT dbo.Listings (ProductID, SellingMethod, UnitPrice, LotPrice, LotUnits, MinOrderQuantity, ListingQuantity, Status, PublishedAt)
SELECT p.ProductID, CASE WHEN x.LotUnits IS NULL THEN N'regular' ELSE N'whole_lot' END,
       CASE WHEN x.LotUnits IS NULL THEN x.Price END, CASE WHEN x.LotUnits IS NOT NULL THEN x.Price END,
       x.LotUnits, 1, x.Quantity, N'active', SYSUTCDATETIME()
FROM @created p JOIN @catalog x ON x.SKU = p.SKU;

COMMIT;

