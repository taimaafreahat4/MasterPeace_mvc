param(
    [Parameter(Mandatory)] [string] $BackupPath,
    [string] $Server = 'localhost'
)
$ErrorActionPreference = 'Stop'
$escapedBackup = $BackupPath.Replace("'", "''")
$query = @"
SET NOCOUNT ON;
IF DB_ID(N'StockMindMVC') IS NOT NULL
    THROW 50010, 'StockMindMVC already exists. Restore requires a fresh database name.', 1;
DECLARE @dir nvarchar(1024) = CONVERT(nvarchar(1024), SERVERPROPERTY('InstanceDefaultDataPath'));
DECLARE @log nvarchar(1024) = CONVERT(nvarchar(1024), SERVERPROPERTY('InstanceDefaultLogPath'));
DECLARE @dataFile nvarchar(1024) = CONCAT(@dir, N'StockMindMVC.mdf');
DECLARE @logFile nvarchar(1024) = CONCAT(@log, N'StockMindMVC_log.ldf');
RESTORE DATABASE [StockMindMVC] FROM DISK=N'$escapedBackup'
WITH MOVE N'StockMindDB' TO @dataFile, MOVE N'StockMindDB_log' TO @logFile, RECOVERY;
"@
& sqlcmd -S $Server -E -C -b -Q $query
if ($LASTEXITCODE -ne 0) { throw 'SQL Server restore failed.' }
& sqlcmd -S $Server -E -C -b -d StockMindMVC -i (Join-Path $PSScriptRoot '01-prepare-catalog.sql')
if ($LASTEXITCODE -ne 0) { throw 'Catalog preparation failed.' }
& sqlcmd -S $Server -E -C -b -d StockMindMVC -i (Join-Path $PSScriptRoot '02-registration-profiles.sql')
if ($LASTEXITCODE -ne 0) { throw 'Registration profile setup failed.' }
& sqlcmd -S $Server -E -C -b -d StockMindMVC -i (Join-Path $PSScriptRoot '03-demo-testimonials.sql')
if ($LASTEXITCODE -ne 0) { throw 'Demo testimonial setup failed.' }
