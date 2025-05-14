@echo off
echo ===================================================
echo POS Database Migration Fix Tool
echo ===================================================
echo This script will fix database migration issues by:
echo 1. Creating a custom SQL migration with USING clause
echo 2. Applying the migration directly to the database
echo.

REM Set your PostgreSQL connection details
set PGHOST=localhost
set PGUSER=pos_user
set PGPASSWORD=rj200100p
set PGDATABASE=pos_system

echo Checking if psql is available...
where psql >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: PostgreSQL command-line tools (psql) not found!
    echo Please make sure PostgreSQL is installed and its bin directory is in your PATH.
    echo.
    echo You can manually run the SQL commands using pgAdmin or another PostgreSQL client.
    echo.
    pause
    exit /b 1
)

echo PostgreSQL tools found. Proceeding with database fix...
echo.

REM Create a temporary SQL file
set tempSqlFile=%TEMP%\fix-migration-%RANDOM%.sql
echo Creating custom migration SQL file: %tempSqlFile%

REM Write SQL commands to the temporary file
echo -- Fix the Type column in Scope table using USING clause> %tempSqlFile%
echo DO $$>> %tempSqlFile%
echo BEGIN>> %tempSqlFile%
echo     -- Check if the Scope table exists>> %tempSqlFile%
echo     IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'scope') THEN>> %tempSqlFile%
echo         -- Check if Type column is text>> %tempSqlFile%
echo         IF EXISTS (>> %tempSqlFile%
echo             SELECT FROM information_schema.columns >> %tempSqlFile%
echo             WHERE table_schema = 'public' >> %tempSqlFile%
echo             AND table_name = 'scope' >> %tempSqlFile%
echo             AND column_name = 'type' >> %tempSqlFile%
echo             AND data_type = 'text'>> %tempSqlFile%
echo         ) THEN>> %tempSqlFile%
echo             -- Alter the Type column with a USING clause to convert string values to integers>> %tempSqlFile%
echo             ALTER TABLE "Scope" >> %tempSqlFile%
echo             ALTER COLUMN "Type" TYPE integer >> %tempSqlFile%
echo             USING CASE >> %tempSqlFile%
echo                 WHEN "Type" = 'Global' THEN 0>> %tempSqlFile%
echo                 WHEN "Type" = 'Store' THEN 1>> %tempSqlFile%
echo                 WHEN "Type" = 'Terminal' THEN 2>> %tempSqlFile%
echo                 ELSE 0 -- Default to Global if unknown>> %tempSqlFile%
echo             END;>> %tempSqlFile%
echo             >> %tempSqlFile%
echo             RAISE NOTICE 'Successfully converted Type column from text to integer';>> %tempSqlFile%
echo         ELSE>> %tempSqlFile%
echo             RAISE NOTICE 'Type column is already an integer or does not exist';>> %tempSqlFile%
echo         END IF;>> %tempSqlFile%
echo     ELSE>> %tempSqlFile%
echo         RAISE NOTICE 'Scope table does not exist';>> %tempSqlFile%
echo     END IF;>> %tempSqlFile%
echo END>> %tempSqlFile%
echo $$;>> %tempSqlFile%
echo.>> %tempSqlFile%
echo -- Mark migrations as applied if needed>> %tempSqlFile%
echo DO $$>> %tempSqlFile%
echo BEGIN>> %tempSqlFile%
echo     -- Check if __EFMigrationsHistory table exists>> %tempSqlFile%
echo     IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '__efmigrationshistory') THEN>> %tempSqlFile%
echo         -- Insert migration records if they don't exist>> %tempSqlFile%
echo         INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")>> %tempSqlFile%
echo         VALUES >> %tempSqlFile%
echo         ('20250513012432_UpdateCustomPackagePrice', '9.0.4'),>> %tempSqlFile%
echo         ('20250514012611_FixWarningsOnly', '9.0.4')>> %tempSqlFile%
echo         ON CONFLICT ("MigrationId") DO NOTHING;>> %tempSqlFile%
echo         >> %tempSqlFile%
echo         RAISE NOTICE 'Migration history updated';>> %tempSqlFile%
echo     ELSE>> %tempSqlFile%
echo         RAISE NOTICE '__EFMigrationsHistory table does not exist';>> %tempSqlFile%
echo     END IF;>> %tempSqlFile%
echo END>> %tempSqlFile%
echo $$;>> %tempSqlFile%

REM Run the SQL script to fix the database
echo Applying custom migration...
psql -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f %tempSqlFile%

if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to execute the SQL script.
    echo Please check your PostgreSQL connection details and try again.
    pause
    exit /b 1
)

echo.
echo ===================================================
echo Database fixed successfully!
echo ===================================================
echo.
echo You can now run migrations normally:
echo   dotnet ef database update
echo.
echo If you still encounter issues, you may need to:
echo 1. Remove all migrations: dotnet ef migrations remove
echo 2. Add a new migration: dotnet ef migrations add FixDatabase
echo 3. Update the database: dotnet ef database update
echo.

pause
