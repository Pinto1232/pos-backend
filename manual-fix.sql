-- Manual fix for database migration issues
-- Run this script in pgAdmin or another PostgreSQL client

-- Fix the Type column in Scope table using USING clause
DO $$
BEGIN
    -- Check if the Scope table exists
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'scope') THEN
        -- Check if Type column is text
        IF EXISTS (
            SELECT FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'scope' 
            AND column_name = 'type' 
            AND data_type = 'text'
        ) THEN
            -- Alter the Type column with a USING clause to convert string values to integers
            ALTER TABLE "Scope" 
            ALTER COLUMN "Type" TYPE integer 
            USING CASE 
                WHEN "Type" = 'Global' THEN 0
                WHEN "Type" = 'Store' THEN 1
                WHEN "Type" = 'Terminal' THEN 2
                ELSE 0 -- Default to Global if unknown
            END;
            
            RAISE NOTICE 'Successfully converted Type column from text to integer';
        ELSE
            RAISE NOTICE 'Type column is already an integer or does not exist';
        END IF;
    ELSE
        RAISE NOTICE 'Scope table does not exist';
    END IF;
END
$$;

-- Fix the AddOns table issue
DO $$
BEGIN
    -- Check if AddOns table already exists
    IF NOT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'addons') THEN
        RAISE NOTICE 'AddOns table does not exist, no action needed';
    ELSE
        RAISE NOTICE 'AddOns table already exists';
    END IF;
END
$$;

-- Mark migrations as applied if needed
DO $$
BEGIN
    -- Check if __EFMigrationsHistory table exists
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '__efmigrationshistory') THEN
        -- Insert migration records if they don't exist
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES 
        ('20250513012432_UpdateCustomPackagePrice', '9.0.4'),
        ('20250514012611_FixWarningsOnly', '9.0.4')
        ON CONFLICT ("MigrationId") DO NOTHING;
        
        RAISE NOTICE 'Migration history updated';
    ELSE
        RAISE NOTICE '__EFMigrationsHistory table does not exist';
    END IF;
END
$$;
