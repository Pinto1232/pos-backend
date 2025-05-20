-- Fix the RegionalSettings issue by adding JSON columns
ALTER TABLE "UserCustomizations" 
ADD COLUMN IF NOT EXISTS "TaxSettingsJson" jsonb NULL,
ADD COLUMN IF NOT EXISTS "RegionalSettingsJson" jsonb NULL;

-- Update the migrations history to mark our migrations as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
('20250505230750_AddOwnedEntitiesConfiguration', '9.0.4'),
('20250505231016_AddJsonColumnsForSettings', '9.0.4')
ON CONFLICT ("MigrationId") DO NOTHING;
