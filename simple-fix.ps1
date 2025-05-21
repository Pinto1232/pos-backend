Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "POS Database Simple Fix Tool" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "This script will fix the database issues by:"
Write-Host "1. Dropping the database"
Write-Host "2. Creating a new database with all migrations except the problematic ones"
Write-Host "3. Applying custom SQL to fix the Type column"
Write-Host ""

Write-Host "Step 1: Dropping the database..." -ForegroundColor Yellow
dotnet ef database drop --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to drop the database." -ForegroundColor Red
    Write-Host "Please check your connection details and try again."
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "Database dropped successfully." -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Creating a new database with migrations..." -ForegroundColor Yellow
Write-Host "This will apply all migrations except the problematic ones."
dotnet ef database update 20250513012432_UpdateCustomPackagePrice
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to apply migrations." -ForegroundColor Red
    Write-Host "Please check your migration files and try again."
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "Migrations applied successfully." -ForegroundColor Green
Write-Host ""

Write-Host "Step 3: Creating SQL script to fix the Type column..." -ForegroundColor Yellow
$sqlScript = @"
-- Fix the Type column in Scope table using USING clause
DO `$`$
BEGIN
    -- Check if the Scope table exists
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'scope') THEN
        -- Check if Type column is text
        IF EXISTS (
            SELECT FROM information_schema.columns
            WHERE table_schema = 'public'
            AND table_name = 'scope'
            AND column_name = 'type'
            AND data_type = 'text'
        ) THEN
            -- Alter the Type column with a USING clause to convert string values to integers
            ALTER TABLE "Scope"
            ALTER COLUMN "Type" TYPE integer
            USING CASE
                WHEN "Type" = 'Global' THEN 0
                WHEN "Type" = 'Store' THEN 1
                WHEN "Type" = 'Terminal' THEN 2
                ELSE 0 -- Default to Global if unknown
            END;

            RAISE NOTICE 'Successfully converted Type column from text to integer';
        ELSE
            RAISE NOTICE 'Type column is already an integer or does not exist';
        END IF;
    ELSE
        RAISE NOTICE 'Scope table does not exist';
    END IF;
END
`$`$;

-- Mark migrations as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
('20250513012432_UpdateCustomPackagePrice', '9.0.4'),
('20250514012611_FixWarningsOnly', '9.0.4')
ON CONFLICT ("MigrationId") DO NOTHING;
"@

$sqlScript | Out-File -FilePath "fix-type-column.sql" -Encoding utf8

Write-Host "Step 4: Applying the SQL script..." -ForegroundColor Yellow
Write-Host "This step requires PostgreSQL command-line tools (psql)."
Write-Host "If you don't have psql, you'll need to run the SQL in fix-type-column.sql manually."
Write-Host ""

$psqlExists = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlExists) {
    Write-Host "WARNING: PostgreSQL command-line tools (psql) not found!" -ForegroundColor Yellow
    Write-Host "Please run the SQL in fix-type-column.sql manually using pgAdmin."
    Write-Host "Then run: dotnet ef database update"
    Write-Host ""
    Write-Host "The SQL script has been saved to fix-type-column.sql"
    Read-Host "Press Enter to exit"
    exit 0
} else {
    Write-Host "PostgreSQL tools found. Applying SQL script..." -ForegroundColor Green
    $env:PGHOST = "localhost"
    $env:PGUSER = "pos_user"
    $env:PGPASSWORD = "rj200100p"
    $env:PGDATABASE = "pos_system"
    psql -h $env:PGHOST -U $env:PGUSER -d $env:PGDATABASE -f fix-type-column.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Failed to apply SQL script." -ForegroundColor Yellow
        Write-Host "Please run the SQL in fix-type-column.sql manually using pgAdmin."
        Write-Host "Then run: dotnet ef database update"
        Write-Host ""
        Write-Host "The SQL script has been saved to fix-type-column.sql"
        Read-Host "Press Enter to exit"
        exit 0
    } else {
        Write-Host "SQL script applied successfully." -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "Step 5: Applying remaining migrations..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Some migrations may have failed." -ForegroundColor Yellow
    Write-Host "You may need to manually fix remaining issues."
} else {
    Write-Host "All migrations applied successfully!" -ForegroundColor Green
}
Write-Host ""

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Fix process completed!" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit"
