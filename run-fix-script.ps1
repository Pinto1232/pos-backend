Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Running SQL Fix Script" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""

# Set PostgreSQL connection details
$PGHOST = "localhost"
$PGUSER = "pos_user"
$PGPASSWORD = "rj200100p"
$PGDATABASE = "pos_system"

# Check if psql is available
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlExists) {
    Write-Host "WARNING: PostgreSQL command-line tools (psql) not found!" -ForegroundColor Yellow
    Write-Host "Please run the SQL in fix-type-column.sql manually using pgAdmin."
    Write-Host ""
    Write-Host "The SQL script is:" -ForegroundColor Yellow
    Write-Host ""
    Get-Content -Path "fix-type-column.sql" | Write-Host
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 0
} else {
    Write-Host "PostgreSQL tools found. Applying SQL script..." -ForegroundColor Green
    $env:PGPASSWORD = $PGPASSWORD
    $result = psql -h $PGHOST -U $PGUSER -d $PGDATABASE -f fix-type-column.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Failed to apply SQL script." -ForegroundColor Yellow
        Write-Host "Please run the SQL in fix-type-column.sql manually using pgAdmin."
        Write-Host ""
        Write-Host "The SQL script is:" -ForegroundColor Yellow
        Write-Host ""
        Get-Content -Path "fix-type-column.sql" | Write-Host
        Write-Host ""
        Read-Host "Press Enter to exit"
        exit 0
    } else {
        Write-Host "SQL script applied successfully." -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Fix script completed!" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Now try running: dotnet ef database update" -ForegroundColor Yellow
Write-Host ""
Read-Host "Press Enter to exit"
