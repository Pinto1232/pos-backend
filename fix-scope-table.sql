-- First, check if the Scope table exists
DO $$
DECLARE
    table_exists BOOLEAN;
    scope_count INTEGER;
BEGIN
    -- Check if the table exists
    SELECT EXISTS (
        SELECT FROM information_schema.tables
        WHERE table_schema = 'public'
        AND table_name = 'Scope'
    ) INTO table_exists;

    IF table_exists THEN
        -- Check if there are any records in the table
        EXECUTE 'SELECT COUNT(*) FROM "Scope"' INTO scope_count;

        IF scope_count > 0 THEN
            -- Backup the existing data
            CREATE TABLE IF NOT EXISTS "Scope_Backup" AS SELECT * FROM "Scope";

            -- Drop the existing table
            DROP TABLE "Scope";

            -- Create a new table with the correct column types
            CREATE TABLE "Scope" (
                "Id" SERIAL PRIMARY KEY,
                "Type" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "Description" TEXT NOT NULL
            );

            -- Insert data from backup with type conversion
            INSERT INTO "Scope" ("Id", "Type", "Name", "Description")
            SELECT
                "Id",
                CASE
                    WHEN "Type" = 'Global' THEN 0
                    WHEN "Type" = 'Store' THEN 1
                    WHEN "Type" = 'Terminal' THEN 2
                    ELSE 0 -- Default to Global if unknown
                END,
                "Name",
                "Description"
            FROM "Scope_Backup";

            -- Reset the sequence to the max ID + 1
            EXECUTE 'SELECT setval(''"Scope_Id_seq"'', (SELECT MAX("Id") FROM "Scope"), true)';
        ELSE
            -- If the table exists but is empty, just drop and recreate it
            DROP TABLE "Scope";

            -- Create a new table with the correct column types
            CREATE TABLE "Scope" (
                "Id" SERIAL PRIMARY KEY,
                "Type" INTEGER NOT NULL,
                "Name" TEXT NOT NULL,
                "Description" TEXT NOT NULL
            );

            -- Insert default values
            INSERT INTO "Scope" ("Type", "Name", "Description")
            VALUES
            (0, 'Global', 'Global scope for system-wide settings'),
            (1, 'Store', 'Store-level scope for store-specific settings'),
            (2, 'Terminal', 'Terminal-level scope for terminal-specific settings');
        END IF;
    ELSE
        -- If the table doesn't exist, create it with the correct column types
        CREATE TABLE "Scope" (
            "Id" SERIAL PRIMARY KEY,
            "Type" INTEGER NOT NULL,
            "Name" TEXT NOT NULL,
            "Description" TEXT NOT NULL
        );

        -- Insert default values
        INSERT INTO "Scope" ("Type", "Name", "Description")
        VALUES
        (0, 'Global', 'Global scope for system-wide settings'),
        (1, 'Store', 'Store-level scope for store-specific settings'),
        (2, 'Terminal', 'Terminal-level scope for terminal-specific settings');
    END IF;
END
$$;

-- Update the migrations history to mark our migrations as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
('20250513012432_UpdateCustomPackagePrice', '9.0.4'),
('20250514012611_FixWarningsOnly', '9.0.4')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Check if we need to create the AddOns table
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'AddOns') THEN
        CREATE TABLE "AddOns" (
            "Id" SERIAL PRIMARY KEY,
            "Name" TEXT NOT NULL,
            "Description" TEXT NOT NULL,
            "Price" NUMERIC NOT NULL
        );

        -- Insert some default add-ons
        INSERT INTO "AddOns" ("Name", "Description", "Price")
        VALUES
        ('Advanced Analytics', 'Detailed business analytics and insights', 15.00),
        ('API Access', 'Access to API for custom integrations', 25.00),
        ('Custom Branding', 'White-label solution with your branding', 20.00),
        ('24/7 Support', 'Round-the-clock customer support', 30.00),
        ('Data Migration', 'Assistance with data migration from other systems', 50.00);
    END IF;
END
$$;
