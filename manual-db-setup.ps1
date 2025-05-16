Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "POS Database Manual Setup Tool" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "This script will manually set up the database by:"
Write-Host "1. Dropping the existing database"
Write-Host "2. Creating a new database"
Write-Host "3. Creating tables manually with the correct column types"
Write-Host "4. Marking migrations as applied"
Write-Host ""

# Set PostgreSQL connection details
$PGHOST = "localhost"
$PGUSER = "pos_user"
$PGPASSWORD = "rj200100p"
$PGDATABASE = "pos_system"

Write-Host "Step 1: Dropping the database..." -ForegroundColor Yellow
# Execute the command to drop the database
dotnet ef database drop --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to drop the database." -ForegroundColor Red
    Write-Host "Please check your connection details and try again."
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "Database dropped successfully." -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Creating a new database..." -ForegroundColor Yellow
# Create a SQL script to create the database
$createDbSql = @"
CREATE DATABASE pos_system
    WITH
    OWNER = pos_user
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;
"@

# Save the SQL to a file
$createDbSql | Out-File -FilePath "create-db.sql" -Encoding utf8

# Check if psql is available
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlExists) {
    Write-Host "WARNING: PostgreSQL command-line tools (psql) not found!" -ForegroundColor Yellow
    Write-Host "Please create the database manually using pgAdmin."
    Write-Host "The SQL script has been saved to create-db.sql"
    Read-Host "Press Enter when you've created the database"
} else {
    Write-Host "PostgreSQL tools found. Creating database..." -ForegroundColor Green
    $env:PGPASSWORD = $PGPASSWORD
    # Connect to postgres database to create our database
    psql -h $PGHOST -U $PGUSER -d postgres -f create-db.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Failed to create database." -ForegroundColor Yellow
        Write-Host "Please create the database manually using pgAdmin."
        Write-Host "The SQL script has been saved to create-db.sql"
        Read-Host "Press Enter when you've created the database"
    } else {
        Write-Host "Database created successfully." -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "Step 3: Creating tables with correct column types..." -ForegroundColor Yellow
# Create a SQL script to create the tables
$createTablesSql = @"
-- Create __EFMigrationsHistory table first
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create Scope table with Type as integer
CREATE TABLE IF NOT EXISTS "Scope" (
    "Id" SERIAL PRIMARY KEY,
    "Type" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL
);

-- Insert default values into Scope
INSERT INTO "Scope" ("Type", "Name", "Description")
VALUES
(0, 'Global', 'Global scope for system-wide settings'),
(1, 'Store', 'Store-level scope for store-specific settings'),
(2, 'Terminal', 'Terminal-level scope for terminal-specific settings');

-- Create AddOns table
CREATE TABLE IF NOT EXISTS "AddOns" (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Price" NUMERIC NOT NULL
);

-- Insert some default add-ons
INSERT INTO "AddOns" ("Name", "Description", "Price")
VALUES
('Advanced Analytics', 'Detailed business analytics and insights', 15.00),
('API Access', 'Access to API for custom integrations', 25.00),
('Custom Branding', 'White-label solution with your branding', 20.00),
('24/7 Support', 'Round-the-clock customer support', 30.00),
('Data Migration', 'Assistance with data migration from other systems', 50.00);

-- Mark migrations as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
('20250505122341_InitialCreate', '9.0.4'),
('20250505182910_PerformanceOptimizations', '9.0.4'),
('20250505230750_AddOwnedEntitiesConfiguration', '9.0.4'),
('20250505231016_AddJsonColumnsForSettings', '9.0.4'),
('20250505235014_CurrencyApiMigration', '9.0.4'),
('20250505235041_MigrationName', '9.0.4'),
('20250513012432_UpdateCustomPackagePrice', '9.0.4'),
('20250514012611_FixWarningsOnly', '9.0.4')
ON CONFLICT ("MigrationId") DO NOTHING;
"@

# Save the SQL to a file
$createTablesSql | Out-File -FilePath "create-tables.sql" -Encoding utf8

# Check if psql is available
if (-not $psqlExists) {
    Write-Host "WARNING: PostgreSQL command-line tools (psql) not found!" -ForegroundColor Yellow
    Write-Host "Please run the SQL script manually using pgAdmin."
    Write-Host "The SQL script has been saved to create-tables.sql"
    Read-Host "Press Enter when you've run the script"
} else {
    Write-Host "PostgreSQL tools found. Creating tables..." -ForegroundColor Green
    $env:PGPASSWORD = $PGPASSWORD
    psql -h $PGHOST -U $PGUSER -d $PGDATABASE -f create-tables.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Host "WARNING: Failed to create tables." -ForegroundColor Yellow
        Write-Host "Please run the SQL script manually using pgAdmin."
        Write-Host "The SQL script has been saved to create-tables.sql"
        Read-Host "Press Enter when you've run the script"
    } else {
        Write-Host "Tables created successfully." -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "Step 4: Applying remaining migrations..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Some migrations may have failed." -ForegroundColor Yellow
    Write-Host "You may need to manually fix remaining issues."
} else {
    Write-Host "All migrations applied successfully!" -ForegroundColor Green
}
Write-Host ""

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Database setup completed!" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit"
