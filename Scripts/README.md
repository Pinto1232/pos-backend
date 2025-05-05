# Performance Optimization Scripts

This directory contains scripts for optimizing and testing the performance of your POS backend.

## Database Optimization Scripts

### Optimize-Database.ps1 (Recommended for Windows)

PowerShell script that automatically finds your PostgreSQL installation and runs the database optimizations.

Usage:
```powershell
.\Optimize-Database.ps1
```

### optimize-database.sh (Linux/macOS/Git Bash)

This script creates database indexes and updates statistics to improve query performance.

Usage:
```bash
chmod +x optimize-database.sh
./optimize-database.sh
```

Note: If using Git Bash on Windows, you'll need to update the `PSQL_PATH` variable in the script to point to your PostgreSQL installation.

### optimize-database.bat (Windows Command Prompt)

Windows batch file version of the database optimization script.

Usage:
```
optimize-database.bat
```

Note: You may need to update the PostgreSQL path in the script if your installation is in a different location.

## Performance Testing Scripts

### test-caching.ps1

This PowerShell script tests the effectiveness of the API response caching by making multiple requests to the same endpoint and measuring the response times.

Usage:
```powershell
.\test-caching.ps1
```

### load-test.ps1

This PowerShell script performs a basic load test on an API endpoint by making concurrent requests and measuring response times.

Usage:
```powershell
.\load-test.ps1
```

## Database Index Creation

### CreateIndexes.sql

This SQL script creates indexes on frequently queried columns to improve database performance.

It can be run directly using psql:
```bash
psql -h <host> -U <username> -d <database> -f CreateIndexes.sql
```

Or through the optimization scripts provided above.

## Customization

You can customize these scripts by:

1. Updating the API endpoints in the testing scripts
2. Adjusting the number of iterations or concurrent requests in the load testing script
3. Adding additional indexes to the CreateIndexes.sql file for your specific query patterns
