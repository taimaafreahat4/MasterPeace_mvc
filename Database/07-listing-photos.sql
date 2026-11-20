IF OBJECT_ID(N'dbo.ListingPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ListingPhotos (
        ProductId bigint NOT NULL PRIMARY KEY REFERENCES dbo.Products(ProductID) ON DELETE CASCADE,
        Content varbinary(max) NOT NULL,
        ContentType nvarchar(20) NOT NULL
    );
END;
