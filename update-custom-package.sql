-- Update the Custom package price
UPDATE "PricingPackages"
SET "Price" = 49.99,
    "MultiCurrencyPrices" = '{"ZAR": 899.99, "EUR": 45.99, "GBP": 39.99}'
WHERE "Type" = 'custom';
