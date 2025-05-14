# PowerShell script to seed pricing packages data

Write-Host "Seeding pricing packages data..." -ForegroundColor Cyan

# Extract connection string from appsettings.json
$appSettings = Get-Content -Raw -Path "appsettings.json" | ConvertFrom-Json
$connectionString = $appSettings.ConnectionStrings.DefaultConnection

# Parse connection string
$connParts = $connectionString -split ";"
$dbHost = ($connParts | Where-Object { $_ -match "Host=" }) -replace "Host=", ""
$dbName = ($connParts | Where-Object { $_ -match "Database=" }) -replace "Database=", ""
$dbUser = ($connParts | Where-Object { $_ -match "Username=" }) -replace "Username=", ""
$dbPass = ($connParts | Where-Object { $_ -match "Password=" }) -replace "Password=", ""

# Set environment variable for PostgreSQL password
$env:PGPASSWORD = $dbPass

# Check if psql is in the PATH
$psqlCommand = Get-Command psql -ErrorAction SilentlyContinue

if ($null -eq $psqlCommand) {
    # Try to find psql in common installation directories
    $possiblePaths = @(
        "C:\Program Files\PostgreSQL\*\bin\psql.exe",
        "C:\Program Files (x86)\PostgreSQL\*\bin\psql.exe"
    )
    
    $psqlPath = $null
    foreach ($path in $possiblePaths) {
        $foundPaths = Resolve-Path $path -ErrorAction SilentlyContinue
        if ($foundPaths) {
            # Get the most recent version if multiple are found
            $psqlPath = $foundPaths | Sort-Object -Descending | Select-Object -First 1 -ExpandProperty Path
            break
        }
    }
    
    if ($null -eq $psqlPath) {
        Write-Host "PostgreSQL command-line tools (psql) not found. Please install PostgreSQL or add it to your PATH." -ForegroundColor Red
        exit 1
    }
    
    $psqlCommand = $psqlPath
}
else {
    $psqlCommand = $psqlCommand.Source
}

Write-Host "Using psql from: $psqlCommand" -ForegroundColor Yellow

# Execute the SQL script
Write-Host "Executing seed-pricing-packages.sql..." -ForegroundColor Yellow
& $psqlCommand -h $dbHost -U $dbUser -d $dbName -f "seed-pricing-packages.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Pricing packages seeded successfully!" -ForegroundColor Green
}
else {
    Write-Host "Error seeding pricing packages. Exit code: $LASTEXITCODE" -ForegroundColor Red
}
