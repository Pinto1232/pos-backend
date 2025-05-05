@echo off
echo Optimizing database performance...

REM Set PostgreSQL path - update this if your PostgreSQL installation is in a different location
set PSQL_PATH=C:\Program Files\PostgreSQL\16\bin

REM Extract connection string from appsettings.json using PowerShell
for /f "tokens=*" %%a in ('powershell -Command "(Get-Content ..\appsettings.json | Select-String -Pattern '\"DefaultConnection\": \"([^\"]+)\"' | ForEach-Object { $_.Matches.Groups[1].Value })"') do set CONNECTION_STRING=%%a

REM Parse connection string
for /f "tokens=*" %%a in ('powershell -Command "$env:CONNECTION_STRING -match 'Host=([^;]+)'; $matches[1]"') do set DB_HOST=%%a
for /f "tokens=*" %%a in ('powershell -Command "$env:CONNECTION_STRING -match 'Database=([^;]+)'; $matches[1]"') do set DB_NAME=%%a
for /f "tokens=*" %%a in ('powershell -Command "$env:CONNECTION_STRING -match 'Username=([^;]+)'; $matches[1]"') do set DB_USER=%%a
for /f "tokens=*" %%a in ('powershell -Command "$env:CONNECTION_STRING -match 'Password=([^;]+)'; $matches[1]"') do set DB_PASS=%%a

echo Connection details:
echo Host: %DB_HOST%
echo Database: %DB_NAME%
echo User: %DB_USER%

REM Run the index creation script
echo Creating indexes...
set PGPASSWORD=%DB_PASS%
"%PSQL_PATH%\psql.exe" -h %DB_HOST% -U %DB_USER% -d %DB_NAME% -f CreateIndexes.sql

REM Vacuum analyze to update statistics
echo Running VACUUM ANALYZE to update statistics...
"%PSQL_PATH%\psql.exe" -h %DB_HOST% -U %DB_USER% -d %DB_NAME% -c "VACUUM ANALYZE;"

echo Database optimization complete!
pause
