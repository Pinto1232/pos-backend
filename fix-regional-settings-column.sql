-- Fix the RegionalSettingsJson column type to ensure it's JSONB
ALTER TABLE "UserCustomizations" 
ALTER COLUMN "RegionalSettingsJson" TYPE jsonb USING "RegionalSettingsJson"::jsonb;

-- Fix the TaxSettingsJson column type to ensure it's JSONB
ALTER TABLE "UserCustomizations" 
ALTER COLUMN "TaxSettingsJson" TYPE jsonb USING "TaxSettingsJson"::jsonb;
