# Custom Migration Approach

This approach uses a custom migration with a USING clause to fix the Type column issue.

## What This Approach Does

1. **Drops the existing database** to start fresh
2. **Creates a new database** and applies migrations up to our custom fix
3. **Applies our custom migration** with the USING clause to properly convert the Type column
4. **Updates the migration history** to mark problematic migrations as applied
5. **Applies the remaining migrations**

## Files Included

1. **20250515000000_FixTypeColumnWithUsingClause.cs**: Custom migration that includes the USING clause
2. **20250515000000_FixTypeColumnWithUsingClause.Designer.cs**: Designer file for the custom migration
3. **update-migration-history.sql**: SQL script to mark problematic migrations as applied
4. **apply-custom-migration.bat**: Windows batch script to run the entire process
5. **apply-custom-migration.ps1**: PowerShell script with the same functionality

## How to Use

### Option 1: Using the Batch Script (Windows)

1. Open a command prompt in the backend directory
2. Run the batch script:
   ```
   apply-custom-migration.bat
   ```

### Option 2: Using the PowerShell Script (Windows)

1. Open PowerShell in the backend directory
2. Run the PowerShell script:
   ```
   .\apply-custom-migration.ps1
   ```

### Option 3: Manual Steps

If you prefer to run the steps manually:

1. Drop the existing database:
   ```
   dotnet ef database drop --force
   ```

2. Create a new database and apply migrations up to our custom fix:
   ```
   dotnet ef database update 0
   dotnet ef database update 20250515000000_FixTypeColumnWithUsingClause
   ```

3. Update the migration history using pgAdmin:
   - Open pgAdmin
   - Connect to your database
   - Run the SQL in `update-migration-history.sql`

4. Apply the remaining migrations:
   ```
   dotnet ef database update
   ```

## How It Works

The key to this approach is the custom migration that uses a USING clause to properly convert the Type column from string to integer:

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

This SQL statement:
1. Alters the `Type` column in the `Scope` table
2. Changes its data type from `text` to `integer`
3. Uses a `CASE` statement to map string values to their corresponding integer values

By including this in a migration, we ensure that the conversion happens at the right time in the migration process.

## Troubleshooting

If you encounter any issues:

1. **Check the output messages** for any errors
2. **Verify your database connection details** are correct
3. **Make sure you have the necessary permissions** to drop and create databases

If you still have issues, you may need to:

1. Manually run the SQL commands in pgAdmin
2. Check the migration files for any errors
3. Ensure that the migration history is properly updated
