@echo off
echo ===================================================
echo POS Database Custom Migration Tool
echo ===================================================
echo This script will apply a custom migration to fix the Type column issue.
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

echo Step 2: Creating a new database...
dotnet ef database update 0
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to create a new database.
    echo Please check your connection details and try again.
    pause
    exit /b 1
)
echo New database created successfully.
echo.

echo Step 3: Applying migrations up to the fix...
dotnet ef database update 20250515000000_FixTypeColumnWithUsingClause
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to apply migrations.
    echo Please check your migration files and try again.
    pause
    exit /b 1
)
echo Migrations applied successfully.
echo.

echo Step 4: Updating migration history...
echo This step requires PostgreSQL command-line tools (psql).
echo If you don't have psql, you'll need to run the SQL in update-migration-history.sql manually.
echo.
where psql >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo WARNING: PostgreSQL command-line tools (psql) not found!
    echo Please run the SQL in update-migration-history.sql manually using pgAdmin.
) else (
    echo PostgreSQL tools found. Updating migration history...
    set PGHOST=localhost
    set PGUSER=pos_user
    set PGPASSWORD=rj200100p
    set PGDATABASE=pos_system
    psql -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f update-migration-history.sql
    if %ERRORLEVEL% neq 0 (
        echo WARNING: Failed to update migration history.
        echo Please run the SQL in update-migration-history.sql manually using pgAdmin.
    ) else (
        echo Migration history updated successfully.
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
echo Migration process completed!
echo ===================================================
echo.
pause
