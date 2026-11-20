SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT dbo.SubscriptionPlans (PlanCode, PlanName, DurationMonths, Price, Currency, IsActive)
SELECT p.Code, p.Name, p.Months, p.Price, N'JOD', 1
FROM (VALUES (N'PREMIUM_1', N'Premium - 1 month', 1, 19),
             (N'PREMIUM_3', N'Premium - 3 months', 3, 49),
             (N'PREMIUM_6', N'Premium - 6 months', 6, 89)) p(Code, Name, Months, Price)
WHERE NOT EXISTS (SELECT 1 FROM dbo.SubscriptionPlans existing WHERE existing.PlanCode = p.Code);

IF OBJECT_ID(N'dbo.SellerFreePlans', N'U') IS NULL
    CREATE TABLE dbo.SellerFreePlans (
        SellerId bigint PRIMARY KEY REFERENCES dbo.SellerProfiles(SellerId),
        RecommendationUsedAt datetime2 NULL
    );

IF OBJECT_ID(N'dbo.SubscriptionCheckouts', N'U') IS NULL
    CREATE TABLE dbo.SubscriptionCheckouts (
        CheckoutId uniqueidentifier PRIMARY KEY,
        SubscriptionId bigint NOT NULL UNIQUE REFERENCES dbo.Subscriptions(SubscriptionId),
        FullName nvarchar(150) NOT NULL,
        Email nvarchar(255) NOT NULL,
        StoreName nvarchar(180) NULL,
        Country nvarchar(100) NOT NULL,
        IsDemo bit NOT NULL
    );

COMMIT;
