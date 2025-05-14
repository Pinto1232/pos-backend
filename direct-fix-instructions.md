# Direct SQL Fix Instructions

This approach directly fixes the Type column using SQL with a USING clause.

## What This Approach Does

1. **Directly modifies the Type column** in the Scope table using a USING clause
2. **Converts string values to integers** based on their meaning
3. **Allows the migration to continue** after the column type is fixed

## Files Included

1. **fix-type-column.sql**: SQL script with the USING clause to fix the Type column
2. **run-fix-script.bat**: Windows batch script to run the SQL script
3. **run-fix-script.ps1**: PowerShell script with the same functionality

## How to Use

### Option 1: Using the PowerShell Script (Recommended)

1. Open PowerShell in the backend directory
2. Run the PowerShell script:
   ```
   .\run-fix-script.ps1
   ```
3. After the script completes successfully, run:
   ```
   dotnet ef database update
   ```

### Option 2: Using the Batch Script

1. Open a command prompt in the backend directory
2. Run the batch script:
   ```
   .\run-fix-script.bat
   ```
3. After the script completes successfully, run:
   ```
   dotnet ef database update
   ```

### Option 3: Manual Steps

If you prefer to run the SQL manually:

1. Open pgAdmin
2. Connect to your database (pos_system)
3. Open a Query Tool
4. Copy and paste the following SQL:
   ```sql
   ALTER TABLE "Scope" 
   ALTER COLUMN "Type" TYPE integer 
   USING CASE 
       WHEN "Type" = 'Global' THEN 0
       WHEN "Type" = 'Store' THEN 1
       WHEN "Type" = 'Terminal' THEN 2
       ELSE 0 -- Default to Global if unknown
   END;
   ```
5. Execute the SQL
6. Run `dotnet ef database update` to continue with the migrations

## How It Works

This approach directly modifies the database using SQL with a USING clause, which tells PostgreSQL how to convert the string values to integers:

- 'Global' → 0
- 'Store' → 1
- 'Terminal' → 2

By running this SQL directly, we bypass the Entity Framework migration that's failing, allowing the migration process to continue.
