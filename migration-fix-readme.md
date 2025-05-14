# Database Migration Fix

This directory contains scripts to fix database migration issues in the POS system.

## Common Migration Issues

### 1. Type Column Conversion Error

The most common error is:

```
42804: column "Type" cannot be cast automatically to type integer
```

This happens because the `Scope` table has a column named `Type` that was originally defined as a string (`text`) in the database, but in the C# model, it's defined as an enum (which maps to an integer). A migration is trying to convert this column from string to integer, but there are existing string values that can't be automatically cast to integers.

PostgreSQL requires a `USING` clause to explicitly tell it how to convert the string values to integers.

### 2. AddOns Table Already Exists

Another common error is:

```
42P07: relation "AddOns" already exists
```

This happens when Entity Framework tries to create the `AddOns` table, but it already exists in your database.

## Fix Scripts

We've provided scripts that use the `USING` clause approach to fix these issues:

1. **fix-database.bat**: Windows batch script that:
   - Creates a temporary SQL file with the necessary `USING` clause
   - Applies the SQL directly to the database
   - Marks problematic migrations as applied in the `__EFMigrationsHistory` table

2. **fix-database.ps1**: PowerShell script with the same functionality as the batch script

The scripts generate SQL that:
- Checks if the `Scope` table exists and if the `Type` column is a text column
- Uses a `USING` clause with a `CASE` statement to convert string values to their corresponding integer values
- Marks the problematic migrations as applied so Entity Framework won't try to apply them again

## How to Use

### Option 1: Using the Batch Script (Windows)

1. Open a command prompt in the backend directory
2. Run the batch script:
   ```
   fix-database.bat
   ```
3. After the script completes, run:
   ```
   dotnet ef database update
   ```

### Option 2: Using the PowerShell Script (Windows)

1. Open PowerShell in the backend directory
2. Run the PowerShell script:
   ```
   .\fix-database.ps1
   ```
3. After the script completes, run:
   ```
   dotnet ef database update
   ```

### Option 3: Manual Fix

If you don't have PostgreSQL command-line tools or prefer to use a GUI:

1. Open pgAdmin or another PostgreSQL client
2. Connect to your database
3. Open the `fix-scope-table.sql` file
4. Execute the SQL commands in the file
5. Run the Entity Framework migration:
   ```
   dotnet ef database update
   ```

## If Problems Persist

If you still encounter issues after running the fix scripts, you may need to:

1. Remove all migrations:
   ```
   dotnet ef migrations remove
   ```

2. Add a new migration:
   ```
   dotnet ef migrations add FixDatabase
   ```

3. Update the database:
   ```
   dotnet ef database update
   ```

This approach will create a fresh migration based on the current state of your models and database.

## Understanding the Fix

The main issue is that the `Scope` model in C# defines `Type` as an enum:

```csharp
public enum ScopeType
{
    Global = 0,
    Store = 1,
    Terminal = 2
}

public class Scope
{
    // ...
    public ScopeType Type { get; set; }
    // ...
}
```

But in the database, it was originally created as a text column with string values like 'Global', 'Store', etc.

### The USING Clause Approach

PostgreSQL provides a `USING` clause that allows you to specify how to convert values during a column type change. Our fix uses this approach:

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

This is a safer approach than dropping and recreating the table because:
- It preserves all existing data
- It doesn't require recreating foreign key relationships
- It's a standard PostgreSQL feature for handling type conversions

After fixing the column type, we mark the problematic migrations as already applied in the `__EFMigrationsHistory` table so Entity Framework won't try to apply them again.
