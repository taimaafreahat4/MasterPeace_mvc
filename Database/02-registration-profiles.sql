SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.RegistrationProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RegistrationProfiles (
        UserId bigint NOT NULL CONSTRAINT PK_RegistrationProfiles PRIMARY KEY,
        Username nvarchar(40) NOT NULL CONSTRAINT UQ_RegistrationProfiles_Username UNIQUE,
        Photo varbinary(max) NULL,
        PhotoType nvarchar(20) NULL,
        CONSTRAINT FK_RegistrationProfiles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserID),
        CONSTRAINT CK_RegistrationProfiles_Photo CHECK (
            (Photo IS NULL AND PhotoType IS NULL)
            OR (Photo IS NOT NULL AND PhotoType IS NOT NULL AND PhotoType IN (N'image/png', N'image/jpeg')
                AND DATALENGTH(Photo) BETWEEN 8 AND 2097152)
        )
    );
END;
