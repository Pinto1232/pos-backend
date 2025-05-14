-- Update the migrations history to mark problematic migrations as applied
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
