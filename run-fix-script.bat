@echo off
echo ===================================================
echo Running SQL Fix Script
echo ===================================================
echo.

REM Set PostgreSQL connection details
set PGHOST=localhost
set PGUSER=pos_user
set PGPASSWORD=rj200100p
set PGDATABASE=pos_system

REM Check if psql is available
where psql >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo WARNING: PostgreSQL command-line tools (psql) not found!
    echo Please run the SQL in fix-type-column.sql manually using pgAdmin.
    echo.
    echo The SQL script is:
    echo.
    type fix-type-column.sql
    echo.
    pause
    exit /b 0
) else (
    echo PostgreSQL tools found. Applying SQL script...
    psql -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f fix-type-column.sql
    if %ERRORLEVEL% neq 0 (
        echo WARNING: Failed to apply SQL script.
        echo Please run the SQL in fix-type-column.sql manually using pgAdmin.
        echo.
        echo The SQL script is:
        echo.
        type fix-type-column.sql
        echo.
        pause
        exit /b 0
    ) else (
        echo SQL script applied successfully.
    )
)
echo.

echo ===================================================
echo Fix script completed!
echo ===================================================
echo.
echo Now try running: dotnet ef database update
echo.
pause
