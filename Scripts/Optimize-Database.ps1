# PowerShell script to optimize the PostgreSQL database

# Display header
Write-Host "PostgreSQL Database Optimization Script" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Try to find PostgreSQL installation
$possiblePaths = @(
    "C:\Program Files\PostgreSQL\16\bin",
    "C:\Program Files\PostgreSQL\15\bin",
    "C:\Program Files\PostgreSQL\14\bin",
    "C:\Program Files\PostgreSQL\13\bin",
    "C:\Program Files\PostgreSQL\12\bin"
)

$psqlPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path "$path\psql.exe") {
        $psqlPath = $path
        break
    }
}

if (-not $psqlPath) {
    Write-Host "PostgreSQL installation not found in common locations." -ForegroundColor Red
    $customPath = Read-Host "Please enter the path to your PostgreSQL bin directory (e.g., C:\Program Files\PostgreSQL\16\bin)"
    
    if (Test-Path "$customPath\psql.exe") {
        $psqlPath = $customPath
    } else {
        Write-Host "psql.exe not found at the specified path. Exiting." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Found PostgreSQL at: $psqlPath" -ForegroundColor Green

# Extract connection string from appsettings.json
$appSettingsPath = Join-Path -Path (Split-Path -Parent $PSScriptRoot) -ChildPath "appsettings.json"
$appSettings = Get-Content -Path $appSettingsPath -Raw | ConvertFrom-Json

$connectionString = $appSettings.ConnectionStrings.DefaultConnection
if (-not $connectionString) {
    Write-Host "Connection string not found in appsettings.json" -ForegroundColor Red
    exit 1
}

# Parse connection string
$dbHost = if ($connectionString -match 'Host=([^;]+)') { $matches[1] } else { "localhost" }
$dbName = if ($connectionString -match 'Database=([^;]+)') { $matches[1] } else { throw "Database name not found in connection string" }
$dbUser = if ($connectionString -match 'Username=([^;]+)') { $matches[1] } else { throw "Username not found in connection string" }
$dbPass = if ($connectionString -match 'Password=([^;]+)') { $matches[1] } else { throw "Password not found in connection string" }

Write-Host "Database connection details:" -ForegroundColor Cyan
Write-Host "  Host: $dbHost"
Write-Host "  Database: $dbName"
Write-Host "  User: $dbUser"
Write-Host ""

# Set environment variable for password
$env:PGPASSWORD = $dbPass

# Run the index creation script
Write-Host "Creating indexes..." -ForegroundColor Cyan
$indexScriptPath = Join-Path -Path $PSScriptRoot -ChildPath "CreateIndexes.sql"
& "$psqlPath\psql.exe" -h $dbHost -U $dbUser -d $dbName -f $indexScriptPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creating indexes. Exit code: $LASTEXITCODE" -ForegroundColor Red
} else {
    Write-Host "Indexes created successfully." -ForegroundColor Green
}

# Run VACUUM ANALYZE
Write-Host "Running VACUUM ANALYZE to update statistics..." -ForegroundColor Cyan
& "$psqlPath\psql.exe" -h $dbHost -U $dbUser -d $dbName -c "VACUUM ANALYZE;"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error running VACUUM ANALYZE. Exit code: $LASTEXITCODE" -ForegroundColor Red
} else {
    Write-Host "VACUUM ANALYZE completed successfully." -ForegroundColor Green
}

# Clean up
$env:PGPASSWORD = ""

Write-Host ""
Write-Host "Database optimization complete!" -ForegroundColor Green
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
