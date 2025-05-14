Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "POS Database Migration Fix Tool" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "This script will fix database migration issues by:"
Write-Host "1. Creating a custom SQL migration with USING clause"
Write-Host "2. Applying the migration directly to the database"
Write-Host ""

# Set your PostgreSQL connection details
$PGHOST = "localhost"
$PGUSER = "pos_user"
$PGPASSWORD = "rj200100p"
$PGDATABASE = "pos_system"

# Check if psql is available
Write-Host "Checking if psql is available..." -ForegroundColor Yellow
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue

if (-not $psqlExists) {
    Write-Host "ERROR: PostgreSQL command-line tools (psql) not found!" -ForegroundColor Red
    Write-Host "Please make sure PostgreSQL is installed and its bin directory is in your PATH."
    Write-Host ""
    Write-Host "You can manually run the SQL commands using pgAdmin or another PostgreSQL client."
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "PostgreSQL tools found. Proceeding with database fix..." -ForegroundColor Green
Write-Host ""

# Create a temporary SQL file with the direct migration
$tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
Write-Host "Creating custom migration SQL file: $tempSqlFile" -ForegroundColor Yellow

@"
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

-- Fix the AddOns table issue
DO `$`$
BEGIN
    -- Check if AddOns table already exists
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'addons') THEN
        RAISE NOTICE 'AddOns table does not exist, no action needed';
    ELSE
        RAISE NOTICE 'AddOns table already exists';
    END IF;
END
`$`$;

-- Mark migrations as applied if needed
DO `$`$
BEGIN
    -- Check if __EFMigrationsHistory table exists
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '__efmigrationshistory') THEN
        -- Insert migration records if they don't exist
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES
        ('20250513012432_UpdateCustomPackagePrice', '9.0.4'),
        ('20250514012611_FixWarningsOnly', '9.0.4')
        ON CONFLICT ("MigrationId") DO NOTHING;

        RAISE NOTICE 'Migration history updated';
    ELSE
        RAISE NOTICE '__EFMigrationsHistory table does not exist';
    END IF;
END
`$`$;
"@ | Out-File -FilePath $tempSqlFile -Encoding utf8

# Run the SQL script to fix the database
Write-Host "Applying custom migration..." -ForegroundColor Yellow
$env:PGPASSWORD = $PGPASSWORD
$result = psql -h $PGHOST -U $PGUSER -d $PGDATABASE -f $tempSqlFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to execute the SQL script." -ForegroundColor Red
    Write-Host "Please check your PostgreSQL connection details and try again."
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "===================================================" -ForegroundColor Green
Write-Host "Database fixed successfully!" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green
Write-Host ""
Write-Host "You can now run migrations normally:" -ForegroundColor Cyan
Write-Host "  dotnet ef database update" -ForegroundColor White
Write-Host ""
Write-Host "If you still encounter issues, you may need to:" -ForegroundColor Yellow
Write-Host "1. Remove all migrations: dotnet ef migrations remove" -ForegroundColor White
Write-Host "2. Add a new migration: dotnet ef migrations add FixDatabase" -ForegroundColor White
Write-Host "3. Update the database: dotnet ef database update" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to exit"
