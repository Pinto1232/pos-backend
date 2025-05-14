-- Create __EFMigrationsHistory table first
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Create Scope table with Type as integer
CREATE TABLE IF NOT EXISTS "Scope" (
    "Id" SERIAL PRIMARY KEY,
    "Type" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL
);

-- Insert default values into Scope
INSERT INTO "Scope" ("Type", "Name", "Description")
VALUES 
(0, 'Global', 'Global scope for system-wide settings'),
(1, 'Store', 'Store-level scope for store-specific settings'),
(2, 'Terminal', 'Terminal-level scope for terminal-specific settings');

-- Create AddOns table
CREATE TABLE IF NOT EXISTS "AddOns" (
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

-- Mark migrations as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
('20250505122341_InitialCreate', '9.0.4'),
('20250505182910_PerformanceOptimizations', '9.0.4'),
('20250505230750_AddOwnedEntitiesConfiguration', '9.0.4'),
('20250505231016_AddJsonColumnsForSettings', '9.0.4'),
('20250505235014_CurrencyApiMigration', '9.0.4'),
('20250505235041_MigrationName', '9.0.4'),
('20250513012432_UpdateCustomPackagePrice', '9.0.4'),
('20250514012611_FixWarningsOnly', '9.0.4')
ON CONFLICT ("MigrationId") DO NOTHING;
