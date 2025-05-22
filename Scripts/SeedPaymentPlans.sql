-- Payment Plans Seeding Script
-- This script populates the PaymentPlans table with default payment plans for USD, EUR, and ZAR currencies

-- Check if payment plans already exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "PaymentPlans") THEN
        -- Insert USD Payment Plans
        INSERT INTO "PaymentPlans" (
            "Name", "Period", "DiscountPercentage", "DiscountLabel", "Description", 
            "IsPopular", "IsDefault", "ValidFrom", "ValidTo", "ApplicableRegions", 
            "ApplicableUserTypes", "Currency", "IsActive", "CreatedAt", "UpdatedAt"
        ) VALUES 
        -- USD Plans
        ('Monthly', '1 month', 0.0000, NULL, 'Pay monthly with full flexibility', false, true, NULL, NULL, '*', '*', 'USD', true, NOW(), NOW()),
        ('Quarterly', '3 months', 0.1000, '10% OFF', 'Save 10% with quarterly billing', false, false, NULL, NULL, '*', '*', 'USD', true, NOW(), NOW()),
        ('Semi-Annual', '6 months', 0.1500, '15% OFF', 'Save 15% with semi-annual billing', true, false, NULL, NULL, '*', '*', 'USD', true, NOW(), NOW()),
        ('Annual', '12 months', 0.2000, '20% OFF', 'Maximum savings with annual billing', false, false, NULL, NULL, '*', '*', 'USD', true, NOW(), NOW()),
        
        -- EUR Plans
        ('Monthly', '1 month', 0.0000, NULL, 'Pay monthly with full flexibility', false, true, NULL, NULL, '*', '*', 'EUR', true, NOW(), NOW()),
        ('Quarterly', '3 months', 0.1000, '10% OFF', 'Save 10% with quarterly billing', false, false, NULL, NULL, '*', '*', 'EUR', true, NOW(), NOW()),
        ('Semi-Annual', '6 months', 0.1500, '15% OFF', 'Save 15% with semi-annual billing', true, false, NULL, NULL, '*', '*', 'EUR', true, NOW(), NOW()),
        ('Annual', '12 months', 0.2000, '20% OFF', 'Maximum savings with annual billing', false, false, NULL, NULL, '*', '*', 'EUR', true, NOW(), NOW()),
        
        -- ZAR Plans
        ('Monthly', '1 month', 0.0000, NULL, 'Pay monthly with full flexibility', false, true, NULL, NULL, '*', '*', 'ZAR', true, NOW(), NOW()),
        ('Quarterly', '3 months', 0.1000, '10% OFF', 'Save 10% with quarterly billing', false, false, NULL, NULL, '*', '*', 'ZAR', true, NOW(), NOW()),
        ('Semi-Annual', '6 months', 0.1500, '15% OFF', 'Save 15% with semi-annual billing', true, false, NULL, NULL, '*', '*', 'ZAR', true, NOW(), NOW()),
        ('Annual', '12 months', 0.2000, '20% OFF', 'Maximum savings with annual billing', false, false, NULL, NULL, '*', '*', 'ZAR', true, NOW(), NOW()),
        
        -- GBP Plans
        ('Monthly', '1 month', 0.0000, NULL, 'Pay monthly with full flexibility', false, true, NULL, NULL, '*', '*', 'GBP', true, NOW(), NOW()),
        ('Quarterly', '3 months', 0.1000, '10% OFF', 'Save 10% with quarterly billing', false, false, NULL, NULL, '*', '*', 'GBP', true, NOW(), NOW()),
        ('Semi-Annual', '6 months', 0.1500, '15% OFF', 'Save 15% with semi-annual billing', true, false, NULL, NULL, '*', '*', 'GBP', true, NOW(), NOW()),
        ('Annual', '12 months', 0.2000, '20% OFF', 'Maximum savings with annual billing', false, false, NULL, NULL, '*', '*', 'GBP', true, NOW(), NOW());

        RAISE NOTICE 'Successfully seeded % payment plans.', (SELECT COUNT(*) FROM "PaymentPlans");
    ELSE
        RAISE NOTICE 'Payment plans already exist. Skipping seeding.';
    END IF;
END $$;

-- Verify the seeded data
SELECT 
    "Currency",
    COUNT(*) as "PlanCount",
    STRING_AGG("Name" || ' (' || ROUND("DiscountPercentage" * 100) || '% off)', ', ' ORDER BY "Id") as "Plans"
FROM "PaymentPlans" 
GROUP BY "Currency" 
ORDER BY "Currency";

-- Show all payment plans
SELECT 
    "Id",
    "Currency",
    "Name",
    "Period",
    "DiscountPercentage",
    "DiscountLabel",
    "IsPopular",
    "IsDefault",
    "IsActive"
FROM "PaymentPlans" 
ORDER BY "Currency", "Id";
