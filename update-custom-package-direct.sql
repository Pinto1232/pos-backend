-- Update the Custom package price directly
UPDATE "PricingPackages"
SET "Price" = 49.99,
    "MultiCurrencyPrices" = '{"ZAR": 899.99, "EUR": 45.99, "GBP": 39.99}'
WHERE "Type" = 'custom';

-- If the Custom package doesn't exist, insert it
INSERT INTO "PricingPackages" ("Title", "Description", "Icon", "ExtraDescription", "Price", "TestPeriodDays", "Type", "Currency", "MultiCurrencyPrices")
SELECT 'Custom', 'Build your own package;Select only what you need;Flexible pricing;Scalable solution;Pay for what you use', 'MUI:SettingsIcon', 'Create a custom solution that fits your exact needs', 49.99, 14, 'custom', 'USD', '{"ZAR": 899.99, "EUR": 45.99, "GBP": 39.99}'
WHERE NOT EXISTS (SELECT 1 FROM "PricingPackages" WHERE "Type" = 'custom');
