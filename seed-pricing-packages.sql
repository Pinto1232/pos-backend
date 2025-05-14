-- Seed data for pricing packages
INSERT INTO "PricingPackages" ("Title", "Description", "Icon", "ExtraDescription", "Price", "TestPeriodDays", "Type", "Currency", "MultiCurrencyPrices")
VALUES 
-- Starter Package
('Starter', 'Basic POS functionality;Inventory management;Single store support;Email support;Basic reporting', 'MUI:StarIcon', 'Perfect for small businesses just getting started', 29.99, 14, 'starter', 'USD', '{"ZAR": 549.99, "EUR": 27.99, "GBP": 23.99}'),

-- Growth Package
('Growth', 'Everything in Starter;Multi-store support;Customer loyalty program;Priority support;Advanced reporting;Employee management', 'MUI:TrendingUpIcon', 'Ideal for growing businesses with multiple locations', 59.99, 14, 'growth', 'USD', '{"ZAR": 999.99, "EUR": 54.99, "GBP": 47.99}'),

-- Premium Package
('Premium', 'Everything in Growth;Advanced inventory forecasting;Custom branding;24/7 support;API access;Advanced analytics;Multi-currency support', 'MUI:DiamondIcon', 'For established businesses requiring advanced features', 99.99, 14, 'premium', 'USD', '{"ZAR": 1799.99, "EUR": 89.99, "GBP": 79.99}'),

-- Enterprise Package
('Enterprise', 'Everything in Premium;Dedicated account manager;Custom development;White-label solution;Unlimited users;Advanced security features;Data migration assistance', 'MUI:BusinessIcon', 'Tailored solutions for large enterprises', 199.99, 30, 'enterprise', 'USD', '{"ZAR": 3499.99, "EUR": 179.99, "GBP": 159.99}'),

-- Custom Package
('Custom', 'Build your own package;Select only what you need;Flexible pricing;Scalable solution;Pay for what you use', 'MUI:SettingsIcon', 'Create a custom solution that fits your exact needs', 0.00, 14, 'custom', 'USD', '{"ZAR": 0.00, "EUR": 0.00, "GBP": 0.00}');

-- Seed data for core features
INSERT INTO "CoreFeatures" ("Name", "Description", "BasePrice", "IsRequired")
VALUES
('Point of Sale', 'Basic POS functionality with sales processing', 10.00, true),
('Inventory Management', 'Track and manage inventory levels', 5.00, true),
('Customer Management', 'Manage customer information and purchase history', 5.00, false),
('Reporting', 'Basic sales and inventory reports', 5.00, false),
('Employee Management', 'Manage employee accounts and permissions', 7.50, false),
('Multi-store Support', 'Support for multiple store locations', 15.00, false),
('Loyalty Program', 'Customer loyalty and rewards program', 10.00, false),
('E-commerce Integration', 'Integration with online store', 20.00, false),
('Mobile Access', 'Access POS from mobile devices', 7.50, false),
('Offline Mode', 'Continue operations when internet is down', 5.00, false);

-- Seed data for add-ons
INSERT INTO "AddOns" ("Name", "Description", "Price")
VALUES
('Advanced Analytics', 'Detailed business analytics and insights', 15.00),
('API Access', 'Access to API for custom integrations', 25.00),
('Custom Branding', 'White-label solution with your branding', 20.00),
('24/7 Support', 'Round-the-clock customer support', 30.00),
('Data Migration', 'Assistance with data migration from other systems', 50.00),
('Training Sessions', 'Personalized training for your team', 40.00),
('Hardware Bundle', 'Compatible POS hardware package', 200.00),
('Advanced Security', 'Enhanced security features and compliance', 15.00),
('Multi-currency Support', 'Support for multiple currencies', 10.00),
('Accounting Integration', 'Integration with popular accounting software', 15.00);

-- Seed data for usage-based pricing
INSERT INTO "UsageBasedPricing" ("FeatureId", "Name", "Unit", "MinValue", "MaxValue", "PricePerUnit")
VALUES
(1, 'Number of Transactions', 'transactions/month', 1000, 100000, 0.01),
(2, 'Number of Products', 'products', 100, 10000, 0.05),
(3, 'Number of Users', 'users', 1, 100, 5.00),
(6, 'Number of Stores', 'stores', 1, 50, 10.00),
(7, 'Number of Loyalty Members', 'members', 100, 10000, 0.02),
(8, 'E-commerce Orders', 'orders/month', 100, 10000, 0.10),
(9, 'Mobile Devices', 'devices', 1, 50, 2.00),
(10, 'Offline Storage', 'GB', 1, 100, 1.00);
