SET XACT_ABORT ON;

IF COL_LENGTH(N'dbo.Orders', N'DeliveryNotes') IS NULL
    ALTER TABLE dbo.Orders ADD DeliveryNotes nvarchar(300) NULL;
