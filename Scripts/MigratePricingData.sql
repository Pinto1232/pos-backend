-- Script to migrate existing pricing data to new PackagePrices table
-- Run this after applying the ImplementPricingBestPractices migration

-- First, seed the Currencies table with default values
INSERT INTO "Currencies" ("Code", "Name", "Symbol", "IsActive", "DecimalPlaces", "CreatedAt", "ExchangeRate")
VALUES 
    ('USD', 'US Dollar', '$', true, 2, NOW(), 1.00),
    ('EUR', 'Euro', '€', true, 2, NOW(), 0.93),
    ('GBP', 'British Pound', '£', true, 2, NOW(), 0.80),
    ('ZAR', 'South African Rand', 'R', true, 2, NOW(), 18.50),
    ('CAD', 'Canadian Dollar', 'C$', true, 2, NOW(), 1.35),
    ('AUD', 'Australian Dollar', 'A$', true, 2, NOW(), 1.50),
    ('JPY', 'Japanese Yen', '¥', true, 0, NOW(), 150.00)
ON CONFLICT ("Code") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Symbol" = EXCLUDED."Symbol",
    "IsActive" = EXCLUDED."IsActive",
    "DecimalPlaces" = EXCLUDED."DecimalPlaces",
    "CreatedAt" = COALESCE("Currencies"."CreatedAt", NOW());

-- Migrate existing pricing data from PricingPackages to PackagePrices
-- Insert base currency prices (USD) from existing Price field
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    COALESCE(NULLIF("Currency", ''), 'USD') as "Currency",
    "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
ON CONFLICT DO NOTHING;

-- Parse and insert multi-currency prices from existing MultiCurrencyPrices JSON field
-- This is a more complex operation that would typically be done in application code
-- For now, we'll add some common conversions manually for existing packages

-- Insert EUR prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'EUR' as "Currency",
    ROUND("Price" * 0.93, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'EUR'
  );

-- Insert GBP prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'GBP' as "Currency",
    ROUND("Price" * 0.80, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'GBP'
  );

-- Insert ZAR prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'ZAR' as "Currency",
    ROUND("Price" * 18.50, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'ZAR'
  );

-- Insert CAD prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'CAD' as "Currency",
    ROUND("Price" * 1.35, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'CAD'
  );

-- Insert AUD prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'AUD' as "Currency",
    ROUND("Price" * 1.50, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'AUD'
  );

-- Insert JPY prices (approximate conversion, no decimals)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'JPY' as "Currency",
    ROUND("Price" * 150, 0) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'JPY'
  );

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_PackagePrices_PackageId_Currency" 
ON "PackagePrices" ("PackageId", "Currency");

CREATE INDEX IF NOT EXISTS "IX_PackagePrices_Currency_ValidUntil" 
ON "PackagePrices" ("Currency", "ValidUntil");

CREATE INDEX IF NOT EXISTS "IX_ExchangeRates_FromCurrency_ToCurrency" 
ON "ExchangeRates" ("FromCurrency", "ToCurrency");

CREATE INDEX IF NOT EXISTS "IX_ExchangeRates_ExpiresAt" 
ON "ExchangeRates" ("ExpiresAt") WHERE "ExpiresAt" IS NOT NULL;

-- Verify the migration
SELECT 
    pp."Title",
    pp."Price" as "OldPrice",
    pp."Currency" as "OldCurrency",
    pp."MultiCurrencyPrices" as "OldMultiCurrencyPrices",
    COUNT(npr."Id") as "NewPriceCount",
    STRING_AGG(npr."Currency" || ':' || npr."Price", ', ' ORDER BY npr."Currency") as "NewPrices"
FROM "PricingPackages" pp
LEFT JOIN "PackagePrices" npr ON pp."Id" = npr."PackageId"
GROUP BY pp."Id", pp."Title", pp."Price", pp."Currency", pp."MultiCurrencyPrices"
ORDER BY pp."Id";-- Script to migrate existing pricing data to new PackagePrices table
-- Run this after applying the ImplementPricingBestPractices migration

-- First, seed the Currencies table with default values
INSERT INTO "Currencies" ("Code", "Name", "Symbol", "IsActive", "DecimalPlaces", "CreatedAt", "ExchangeRate")
VALUES 
    ('USD', 'US Dollar', '$', true, 2, NOW(), 1.00),
    ('EUR', 'Euro', '€', true, 2, NOW(), 0.93),
    ('GBP', 'British Pound', '£', true, 2, NOW(), 0.80),
    ('ZAR', 'South African Rand', 'R', true, 2, NOW(), 18.50),
    ('CAD', 'Canadian Dollar', 'C$', true, 2, NOW(), 1.35),
    ('AUD', 'Australian Dollar', 'A$', true, 2, NOW(), 1.50),
    ('JPY', 'Japanese Yen', '¥', true, 0, NOW(), 150.00)
ON CONFLICT ("Code") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Symbol" = EXCLUDED."Symbol",
    "IsActive" = EXCLUDED."IsActive",
    "DecimalPlaces" = EXCLUDED."DecimalPlaces",
    "CreatedAt" = COALESCE("Currencies"."CreatedAt", NOW());

-- Migrate existing pricing data from PricingPackages to PackagePrices
-- Insert base currency prices (USD) from existing Price field
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    COALESCE(NULLIF("Currency", ''), 'USD') as "Currency",
    "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
ON CONFLICT DO NOTHING;

-- Parse and insert multi-currency prices from existing MultiCurrencyPrices JSON field
-- This is a more complex operation that would typically be done in application code
-- For now, we'll add some common conversions manually for existing packages

-- Insert EUR prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'EUR' as "Currency",
    ROUND("Price" * 0.93, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'EUR'
  );

-- Insert GBP prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'GBP' as "Currency",
    ROUND("Price" * 0.80, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'GBP'
  );

-- Insert ZAR prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'ZAR' as "Currency",
    ROUND("Price" * 18.50, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'ZAR'
  );

-- Insert CAD prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'CAD' as "Currency",
    ROUND("Price" * 1.35, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'CAD'
  );

-- Insert AUD prices (approximate conversion)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'AUD' as "Currency",
    ROUND("Price" * 1.50, 2) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'AUD'
  );

-- Insert JPY prices (approximate conversion, no decimals)
INSERT INTO "PackagePrices" ("PackageId", "Currency", "Price", "CreatedAt")
SELECT 
    "Id" as "PackageId",
    'JPY' as "Currency",
    ROUND("Price" * 150, 0) as "Price",
    NOW() as "CreatedAt"
FROM "PricingPackages"
WHERE "Price" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "PackagePrices" 
    WHERE "PackageId" = "PricingPackages"."Id" AND "Currency" = 'JPY'
  );

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_PackagePrices_PackageId_Currency" 
ON "PackagePrices" ("PackageId", "Currency");

CREATE INDEX IF NOT EXISTS "IX_PackagePrices_Currency_ValidUntil" 
ON "PackagePrices" ("Currency", "ValidUntil");

CREATE INDEX IF NOT EXISTS "IX_ExchangeRates_FromCurrency_ToCurrency" 
ON "ExchangeRates" ("FromCurrency", "ToCurrency");

CREATE INDEX IF NOT EXISTS "IX_ExchangeRates_ExpiresAt" 
ON "ExchangeRates" ("ExpiresAt") WHERE "ExpiresAt" IS NOT NULL;

-- Verify the migration
SELECT 
    pp."Title",
    pp."Price" as "OldPrice",
    pp."Currency" as "OldCurrency",
    pp."MultiCurrencyPrices" as "OldMultiCurrencyPrices",
    COUNT(npr."Id") as "NewPriceCount",
    STRING_AGG(npr."Currency" || ':' || npr."Price", ', ' ORDER BY npr."Currency") as "NewPrices"
FROM "PricingPackages" pp
LEFT JOIN "PackagePrices" npr ON pp."Id" = npr."PackageId"
GROUP BY pp."Id", pp."Title", pp."Price", pp."Currency", pp."MultiCurrencyPrices"
ORDER BY pp."Id";