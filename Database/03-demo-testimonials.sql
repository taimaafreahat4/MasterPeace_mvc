SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() NOT IN (N'StockMindMVC', N'StockMindMVC_Test')
    THROW 50010, 'Use the StockMind MVC database.', 1;

BEGIN TRANSACTION;

INSERT dbo.SiteFeedback (SourceType, DisplayName, Category, Rating, Message, ApprovalStatus)
SELECT N'demo_testimonial', sample.DisplayName, sample.Category, 5, sample.Message, N'approved'
FROM (VALUES
    (N'Yousef Haddad', N'demo-electronics', N'Location and season analysis helps me spot what customers need. My shelves are finally stocked with products that sell.'),
    (N'Sarah Ahmed', N'demo-retail', N'Stock Mind helped me avoid costly overstocking. The recommendations make it easier to choose the right products.'),
    (N'Mohammed Ali', N'demo-online', N'I finally know what to buy and when. The demand prediction feature is a game changer.'),
    (N'Lina Hassan', N'demo-business', N'The marketplace is amazing. I found great deals and moved surplus stock much faster.'),
    (N'Omar Khalil', N'demo-wholesale', N'Before every big order, I check Stock Mind. It has helped me make more confident buying decisions.')
) AS sample(DisplayName, Category, Message)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.SiteFeedback WITH (UPDLOCK, HOLDLOCK)
    WHERE SourceType = N'demo_testimonial' AND Category = sample.Category
);

COMMIT;
