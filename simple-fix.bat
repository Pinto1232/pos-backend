@echo off
echo ===================================================
echo POS Database Simple Fix Tool
echo ===================================================
echo This script will fix the database issues by:
echo 1. Dropping the database
echo 2. Creating a new database with all migrations except the problematic ones
echo 3. Applying custom SQL to fix the Type column
echo.

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

echo Step 2: Creating a new database with migrations...
echo This will apply all migrations except the problematic ones.
dotnet ef database update 20250513012432_UpdateCustomPackagePrice
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to apply migrations.
    echo Please check your migration files and try again.
    pause
    exit /b 1
)
echo Migrations applied successfully.
echo.

echo Step 3: Creating SQL script to fix the Type column...
echo -- Fix the Type column in Scope table using USING clause > fix-type-column.sql
echo DO $$ >> fix-type-column.sql
echo BEGIN >> fix-type-column.sql
echo     -- Check if the Scope table exists >> fix-type-column.sql
echo     IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'scope') THEN >> fix-type-column.sql
echo         -- Check if Type column is text >> fix-type-column.sql
echo         IF EXISTS ( >> fix-type-column.sql
echo             SELECT FROM information_schema.columns >> fix-type-column.sql
echo             WHERE table_schema = 'public' >> fix-type-column.sql
echo             AND table_name = 'scope' >> fix-type-column.sql
echo             AND column_name = 'type' >> fix-type-column.sql
echo             AND data_type = 'text' >> fix-type-column.sql
echo         ) THEN >> fix-type-column.sql
echo             -- Alter the Type column with a USING clause to convert string values to integers >> fix-type-column.sql
echo             ALTER TABLE "Scope" >> fix-type-column.sql
echo             ALTER COLUMN "Type" TYPE integer >> fix-type-column.sql
echo             USING CASE >> fix-type-column.sql
echo                 WHEN "Type" = 'Global' THEN 0 >> fix-type-column.sql
echo                 WHEN "Type" = 'Store' THEN 1 >> fix-type-column.sql
echo                 WHEN "Type" = 'Terminal' THEN 2 >> fix-type-column.sql
echo                 ELSE 0 -- Default to Global if unknown >> fix-type-column.sql
echo             END; >> fix-type-column.sql
echo             >> fix-type-column.sql
echo             RAISE NOTICE 'Successfully converted Type column from text to integer'; >> fix-type-column.sql
echo         ELSE >> fix-type-column.sql
echo             RAISE NOTICE 'Type column is already an integer or does not exist'; >> fix-type-column.sql
echo         END IF; >> fix-type-column.sql
echo     ELSE >> fix-type-column.sql
echo         RAISE NOTICE 'Scope table does not exist'; >> fix-type-column.sql
echo     END IF; >> fix-type-column.sql
echo END >> fix-type-column.sql
echo $$; >> fix-type-column.sql
echo. >> fix-type-column.sql
echo -- Mark migrations as applied >> fix-type-column.sql
echo INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") >> fix-type-column.sql
echo VALUES >> fix-type-column.sql
echo ('20250513012432_UpdateCustomPackagePrice', '9.0.4'), >> fix-type-column.sql
echo ('20250514012611_FixWarningsOnly', '9.0.4') >> fix-type-column.sql
echo ON CONFLICT ("MigrationId") DO NOTHING; >> fix-type-column.sql

echo Step 4: Applying the SQL script...
echo This step requires PostgreSQL command-line tools (psql).
echo If you don't have psql, you'll need to run the SQL in fix-type-column.sql manually.
echo.
where psql >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo WARNING: PostgreSQL command-line tools (psql) not found!
    echo Please run the SQL in fix-type-column.sql manually using pgAdmin.
    echo Then run: dotnet ef database update
    echo.
    echo The SQL script has been saved to fix-type-column.sql
    pause
    exit /b 0
) else (
    echo PostgreSQL tools found. Applying SQL script...
    set PGHOST=localhost
    set PGUSER=pos_user
    set PGPASSWORD=rj200100p
    set PGDATABASE=pos_system
    psql -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f fix-type-column.sql
    if %ERRORLEVEL% neq 0 (
        echo WARNING: Failed to apply SQL script.
        echo Please run the SQL in fix-type-column.sql manually using pgAdmin.
        echo Then run: dotnet ef database update
        echo.
        echo The SQL script has been saved to fix-type-column.sql
        pause
        exit /b 0
    ) else (
        echo SQL script applied successfully.
    )
)
echo.

echo Step 5: Applying remaining migrations...
dotnet ef database update
if %ERRORLEVEL% neq 0 (
    echo WARNING: Some migrations may have failed.
    echo You may need to manually fix remaining issues.
) else (
    echo All migrations applied successfully!
)
echo.

echo ===================================================
echo Fix process completed!
echo ===================================================
echo.
pause
