@echo off
echo ===================================================
echo POS Database Manual Setup Tool
echo ===================================================
echo This script will manually set up the database by:
echo 1. Dropping the existing database
echo 2. Creating a new database
echo 3. Creating tables manually with the correct column types
echo 4. Marking migrations as applied
echo.

REM Set PostgreSQL connection details
set PGHOST=localhost
set PGUSER=pos_user
set PGPASSWORD=rj200100p
set PGDATABASE=pos_system

echo Step 1: Dropping the database...
dotnet ef database drop --force
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to drop the database.
    echo Please check your connection details and try again.
    pause
    exit /b 1
)
echo Database dropped successfully.
echo.

echo Step 2: Creating a new database...
REM Create a SQL script to create the database
echo CREATE DATABASE pos_system> create-db.sql
echo     WITH>> create-db.sql
echo     OWNER = pos_user>> create-db.sql
echo     ENCODING = 'UTF8'>> create-db.sql
echo     LC_COLLATE = 'en_US.UTF-8'>> create-db.sql
echo     LC_CTYPE = 'en_US.UTF-8'>> create-db.sql
echo     TABLESPACE = pg_default>> create-db.sql
echo     CONNECTION LIMIT = -1;>> create-db.sql

REM Check if psql is available
where psql >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo WARNING: PostgreSQL command-line tools (psql) not found!
    echo Please create the database manually using pgAdmin.
    echo The SQL script has been saved to create-db.sql
    pause
) else (
    echo PostgreSQL tools found. Creating database...
    REM Connect to postgres database to create our database
    psql -h %PGHOST% -U %PGUSER% -d postgres -f create-db.sql
    if %ERRORLEVEL% neq 0 (
        echo WARNING: Failed to create database.
        echo Please create the database manually using pgAdmin.
        echo The SQL script has been saved to create-db.sql
        pause
    ) else (
        echo Database created successfully.
    )
)
echo.

echo Step 3: Creating tables with correct column types...
REM Create a SQL script to create the tables
echo -- Create __EFMigrationsHistory table first> create-tables.sql
echo CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (>> create-tables.sql
echo     "MigrationId" VARCHAR(150) NOT NULL,>> create-tables.sql
echo     "ProductVersion" VARCHAR(32) NOT NULL,>> create-tables.sql
echo     CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")>> create-tables.sql
echo );>> create-tables.sql
echo.>> create-tables.sql
echo -- Create Scope table with Type as integer>> create-tables.sql
echo CREATE TABLE IF NOT EXISTS "Scope" (>> create-tables.sql
echo     "Id" SERIAL PRIMARY KEY,>> create-tables.sql
echo     "Type" INTEGER NOT NULL,>> create-tables.sql
echo     "Name" TEXT NOT NULL,>> create-tables.sql
echo     "Description" TEXT NOT NULL>> create-tables.sql
echo );>> create-tables.sql
echo.>> create-tables.sql
echo -- Insert default values into Scope>> create-tables.sql
echo INSERT INTO "Scope" ("Type", "Name", "Description")>> create-tables.sql
echo VALUES >> create-tables.sql
echo (0, 'Global', 'Global scope for system-wide settings'),>> create-tables.sql
echo (1, 'Store', 'Store-level scope for store-specific settings'),>> create-tables.sql
echo (2, 'Terminal', 'Terminal-level scope for terminal-specific settings');>> create-tables.sql
echo.>> create-tables.sql
echo -- Create AddOns table>> create-tables.sql
echo CREATE TABLE IF NOT EXISTS "AddOns" (>> create-tables.sql
echo     "Id" SERIAL PRIMARY KEY,>> create-tables.sql
echo     "Name" TEXT NOT NULL,>> create-tables.sql
echo     "Description" TEXT NOT NULL,>> create-tables.sql
echo     "Price" NUMERIC NOT NULL>> create-tables.sql
echo );>> create-tables.sql
echo.>> create-tables.sql
echo -- Insert some default add-ons>> create-tables.sql
echo INSERT INTO "AddOns" ("Name", "Description", "Price")>> create-tables.sql
echo VALUES >> create-tables.sql
echo ('Advanced Analytics', 'Detailed business analytics and insights', 15.00),>> create-tables.sql
echo ('API Access', 'Access to API for custom integrations', 25.00),>> create-tables.sql
echo ('Custom Branding', 'White-label solution with your branding', 20.00),>> create-tables.sql
echo ('24/7 Support', 'Round-the-clock customer support', 30.00),>> create-tables.sql
echo ('Data Migration', 'Assistance with data migration from other systems', 50.00);>> create-tables.sql
echo.>> create-tables.sql
echo -- Mark migrations as applied>> create-tables.sql
echo INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")>> create-tables.sql
echo VALUES >> create-tables.sql
echo ('20250505122341_InitialCreate', '9.0.4'),>> create-tables.sql
echo ('20250505182910_PerformanceOptimizations', '9.0.4'),>> create-tables.sql
echo ('20250505230750_AddOwnedEntitiesConfiguration', '9.0.4'),>> create-tables.sql
echo ('20250505231016_AddJsonColumnsForSettings', '9.0.4'),>> create-tables.sql
echo ('20250505235014_CurrencyApiMigration', '9.0.4'),>> create-tables.sql
echo ('20250505235041_MigrationName', '9.0.4'),>> create-tables.sql
echo ('20250513012432_UpdateCustomPackagePrice', '9.0.4'),>> create-tables.sql
echo ('20250514012611_FixWarningsOnly', '9.0.4')>> create-tables.sql
echo ON CONFLICT ("MigrationId") DO NOTHING;>> create-tables.sql

REM Check if psql is available
where psql >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo WARNING: PostgreSQL command-line tools (psql) not found!
    echo Please run the SQL script manually using pgAdmin.
    echo The SQL script has been saved to create-tables.sql
    pause
) else (
    echo PostgreSQL tools found. Creating tables...
    psql -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f create-tables.sql
    if %ERRORLEVEL% neq 0 (
        echo WARNING: Failed to create tables.
        echo Please run the SQL script manually using pgAdmin.
        echo The SQL script has been saved to create-tables.sql
        pause
    ) else (
        echo Tables created successfully.
    )
)
echo.

echo Step 4: Applying remaining migrations...
dotnet ef database update
if %ERRORLEVEL% neq 0 (
    echo WARNING: Some migrations may have failed.
    echo You may need to manually fix remaining issues.
) else (
    echo All migrations applied successfully!
)
echo.

echo ===================================================
echo Database setup completed!
echo ===================================================
echo.
pause
