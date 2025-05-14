Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "POS Database Custom Migration Tool" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "This script will apply a custom migration to fix the Type column issue."
Write-Host ""

Write-Host "Step 1: Dropping the database..." -ForegroundColor Yellow
$result = dotnet ef database drop --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to drop the database." -ForegroundColor Red
    Write-Host "Please check your connection details and try again."
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "Database dropped successfully." -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Creating a new database..." -ForegroundColor Yellow
$result = dotnet ef database update 0
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create a new database." -ForegroundColor Red
    Write-Host "Please check your connection details and try again."
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "New database created successfully." -ForegroundColor Green
Write-Host ""

Write-Host "Step 3: Applying migrations up to the fix..." -ForegroundColor Yellow
$result = dotnet ef database update 20250515000000_FixTypeColumnWithUsingClause
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to apply migrations." -ForegroundColor Red
    Write-Host "Please check your migration files and try again."
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "Migrations applied successfully." -ForegroundColor Green
Write-Host ""

Write-Host "Step 4: Updating migration history..." -ForegroundColor Yellow
Write-Host "This step requires PostgreSQL command-line tools (psql)."
Write-Host "If you don't have psql, you'll need to run the SQL in update-migration-history.sql manually."
Write-Host ""

$psqlExists = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlExists) {
    Write-Host "WARNING: PostgreSQL command-line tools (psql) not found!" -ForegroundColor Yellow
    Write-Host "Please run the SQL in update-migration-history.sql manually using pgAdmin."
} else {
    Write-Host "PostgreSQL tools found. Updating migration history..." -ForegroundColor Green
    $env:PGHOST = "localhost"
    $env:PGUSER = "pos_user"
    $env:PGPASSWORD = "rj200100p"
    $env:PGDATABASE = "pos_system"
    $result = psql -h $env:PGHOST -U $env:PGUSER -d $env:PGDATABASE -f update-migration-history.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Failed to update migration history." -ForegroundColor Yellow
        Write-Host "Please run the SQL in update-migration-history.sql manually using pgAdmin."
    } else {
        Write-Host "Migration history updated successfully." -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "Step 5: Applying remaining migrations..." -ForegroundColor Yellow
$result = dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Some migrations may have failed." -ForegroundColor Yellow
    Write-Host "You may need to manually fix remaining issues."
} else {
    Write-Host "All migrations applied successfully!" -ForegroundColor Green
}
Write-Host ""

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Migration process completed!" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit"
