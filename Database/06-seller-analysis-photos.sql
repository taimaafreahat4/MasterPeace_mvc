SET XACT_ABORT ON;
IF COL_LENGTH(N'dbo.AdvisorAnalyses', N'Photo') IS NULL
    ALTER TABLE dbo.AdvisorAnalyses ADD Photo varbinary(max) NULL, PhotoType nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.RecommendationAnalyses', N'Photo') IS NULL
    ALTER TABLE dbo.RecommendationAnalyses ADD Photo varbinary(max) NULL, PhotoType nvarchar(20) NULL;
