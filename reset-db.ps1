# PowerShell script to reset the database and migrations

Write-Host "Resetting database and migrations..." -ForegroundColor Cyan

# 1. Drop the database
Write-Host "Dropping the database..." -ForegroundColor Yellow
dotnet ef database drop --force

# 2. Remove all existing migrations
Write-Host "Removing existing migrations..." -ForegroundColor Yellow
$migrationsDir = "Infrastructure/Data/Migrations"
if (Test-Path $migrationsDir) {
    Remove-Item -Path $migrationsDir\* -Recurse -Force
}

# 3. Create a new initial migration
Write-Host "Creating a new initial migration..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate

# 4. Apply the migration
Write-Host "Applying the migration..." -ForegroundColor Yellow
dotnet ef database update

Write-Host "Database reset complete!" -ForegroundColor Green
