@echo off
setlocal
cd /d "%~dp0"
dotnet watch --project StockMind.csproj --non-interactive run --launch-profile http
exit /b %ERRORLEVEL%
