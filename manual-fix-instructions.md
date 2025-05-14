# Manual Database Fix Instructions

Since you don't have PostgreSQL command-line tools (psql) in your PATH, you'll need to run the SQL script manually using pgAdmin or another PostgreSQL client.

## Steps to Fix the Database

1. **Open pgAdmin** or your preferred PostgreSQL client

2. **Connect to your database**:
   - Host: localhost
   - Database: pos_system
   - Username: pos_user
   - Password: rj200100p

3. **Open the SQL Editor** in pgAdmin:
   - Right-click on your database (pos_system)
   - Select "Query Tool"

4. **Load the SQL script**:
   - Open the file `manual-fix.sql` from this directory
   - Or copy and paste the contents into the query editor

5. **Execute the script**:
   - Click the "Execute" button (or press F5)
   - You should see notices in the output panel indicating what actions were taken

6. **Run Entity Framework migrations**:
   - After the SQL script completes successfully, go back to your command prompt
   - Run: `dotnet ef database update`

## What the Script Does

The SQL script performs the following actions:

1. **Fixes the Type column in the Scope table**:
   - Checks if the Scope table exists and if the Type column is a text column
   - Uses a USING clause with a CASE statement to convert string values to their corresponding integer values:
     - 'Global' → 0
     - 'Store' → 1
     - 'Terminal' → 2

2. **Checks for the AddOns table**:
   - Verifies if the AddOns table already exists
   - No action is needed if it exists, as this is just to prevent the "table already exists" error

3. **Updates the migrations history**:
   - Marks the problematic migrations as applied in the __EFMigrationsHistory table
   - This prevents Entity Framework from trying to apply these migrations again

## Troubleshooting

If you encounter any issues:

1. **Check the output messages** in pgAdmin for any errors
2. **Verify your database connection details** are correct
3. **Make sure you have the necessary permissions** to alter tables and insert records

If you still have issues after running the script, you may need to:

1. Remove all migrations: `dotnet ef migrations remove`
2. Add a new migration: `dotnet ef migrations add FixDatabase`
3. Update the database: `dotnet ef database update`
