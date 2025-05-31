-- Performance optimization: Add indexes to frequently queried columns

-- PricingPackages table indexes (confirmed to exist)
CREATE INDEX IF NOT EXISTS idx_pricing_packages_name ON "PricingPackages" ("Name");

-- Users table indexes (if exists)
CREATE INDEX IF NOT EXISTS idx_users_email ON "Users" ("Email");
CREATE INDEX IF NOT EXISTS idx_users_username ON "Users" ("Username");

-- AspNetUsers table indexes (if using ASP.NET Identity)
CREATE INDEX IF NOT EXISTS idx_aspnetusers_email ON "AspNetUsers" ("Email");
CREATE INDEX IF NOT EXISTS idx_aspnetusers_username ON "AspNetUsers" ("UserName");
CREATE INDEX IF NOT EXISTS idx_aspnetusers_normalized_email ON "AspNetUsers" ("NormalizedEmail");

-- Add indexes for timestamp columns that are commonly used for filtering
CREATE INDEX IF NOT EXISTS idx_created_at ON "PricingPackages" ("CreatedAt");

-- Add indexes for any foreign key relationships
CREATE INDEX IF NOT EXISTS idx_pricing_features_package_id ON "PricingFeatures" ("PricingPackageId");

-- Product and Inventory related indexes
CREATE INDEX IF NOT EXISTS idx_products_category_id ON "Products" ("CategoryId");
CREATE INDEX IF NOT EXISTS idx_products_supplier_id ON "Products" ("SupplierId");
CREATE INDEX IF NOT EXISTS idx_products_name ON "Products" ("Name");
CREATE INDEX IF NOT EXISTS idx_product_variants_product_id ON "ProductVariants" ("ProductId");

-- Inventory indexes
CREATE INDEX IF NOT EXISTS idx_inventories_store_id ON "Inventories" ("StoreId");
CREATE INDEX IF NOT EXISTS idx_inventories_product_variant_id ON "Inventories" ("ProductVariantId");
CREATE INDEX IF NOT EXISTS idx_inventories_quantity ON "Inventories" ("Quantity");
CREATE INDEX IF NOT EXISTS idx_inventories_reorder_level ON "Inventories" ("ReorderLevel");
CREATE INDEX IF NOT EXISTS idx_inventories_low_stock ON "Inventories" ("Quantity", "ReorderLevel");

-- Stock Alerts indexes
CREATE INDEX IF NOT EXISTS idx_stock_alerts_inventory_id ON "StockAlerts" ("InventoryId");
CREATE INDEX IF NOT EXISTS idx_stock_alerts_is_active ON "StockAlerts" ("IsActive");
CREATE INDEX IF NOT EXISTS idx_stock_alerts_active_inventory ON "StockAlerts" ("IsActive", "InventoryId");

-- Customer related indexes
CREATE INDEX IF NOT EXISTS idx_customers_email ON "Customers" ("Email");
CREATE INDEX IF NOT EXISTS idx_customers_phone ON "Customers" ("Phone");
CREATE INDEX IF NOT EXISTS idx_customer_group_members_customer_id ON "CustomerGroupMembers" ("CustomerId");
CREATE INDEX IF NOT EXISTS idx_customer_group_members_group_id ON "CustomerGroupMembers" ("CustomerGroupId");

-- Sales and Orders indexes
CREATE INDEX IF NOT EXISTS idx_sales_customer_id ON "Sales" ("CustomerId");
CREATE INDEX IF NOT EXISTS idx_sales_date ON "Sales" ("SaleDate");
CREATE INDEX IF NOT EXISTS idx_sale_items_sale_id ON "SaleItems" ("SaleId");
CREATE INDEX IF NOT EXISTS idx_sale_items_product_variant_id ON "SaleItems" ("ProductVariantId");

CREATE INDEX IF NOT EXISTS idx_orders_customer_id ON "Orders" ("CustomerId");
CREATE INDEX IF NOT EXISTS idx_orders_date ON "Orders" ("OrderDate");
CREATE INDEX IF NOT EXISTS idx_order_items_order_id ON "OrderItems" ("OrderId");
CREATE INDEX IF NOT EXISTS idx_order_items_product_variant_id ON "OrderItems" ("ProductVariantId");

-- User Subscription indexes
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_user_id ON "UserSubscriptions" ("UserId");
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_package_id ON "UserSubscriptions" ("PricingPackageId");
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_active ON "UserSubscriptions" ("IsActive");
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_start_date ON "UserSubscriptions" ("StartDate");

-- Payment related indexes
CREATE INDEX IF NOT EXISTS idx_payments_customer_id ON "Payments" ("CustomerId");
CREATE INDEX IF NOT EXISTS idx_payments_date ON "Payments" ("PaymentDate");
CREATE INDEX IF NOT EXISTS idx_payment_method_infos_user_id ON "PaymentMethodInfos" ("UserId");
CREATE INDEX IF NOT EXISTS idx_payment_notifications_user_id ON "PaymentNotificationHistories" ("UserId");

-- Feature Flags and Access Control indexes
CREATE INDEX IF NOT EXISTS idx_feature_flags_name ON "FeatureFlags" ("FeatureName");
CREATE INDEX IF NOT EXISTS idx_feature_flags_enabled ON "FeatureFlags" ("IsEnabled");
CREATE INDEX IF NOT EXISTS idx_user_feature_usage_user_id ON "UserFeatureUsages" ("UserId");
CREATE INDEX IF NOT EXISTS idx_user_feature_usage_feature_id ON "UserFeatureUsages" ("FeatureFlagId");
CREATE INDEX IF NOT EXISTS idx_feature_access_logs_user_id ON "FeatureAccessLogs" ("UserId");
CREATE INDEX IF NOT EXISTS idx_feature_access_logs_feature_name ON "FeatureAccessLogs" ("FeatureName");

-- Composite indexes for common query patterns
CREATE INDEX IF NOT EXISTS idx_sales_customer_date ON "Sales" ("CustomerId", "SaleDate");
CREATE INDEX IF NOT EXISTS idx_orders_customer_date ON "Orders" ("CustomerId", "OrderDate");
CREATE INDEX IF NOT EXISTS idx_user_subscriptions_user_active ON "UserSubscriptions" ("UserId", "IsActive");

-- Note: This script has been customized based on the tables that were confirmed to exist
-- in your database. You can add more indexes as you identify performance bottlenecks.
--
-- Common patterns to index:
-- 1. Foreign keys (columns ending with 'Id')
-- 2. Columns used in WHERE clauses
-- 3. Columns used in JOIN conditions
-- 4. Columns used in ORDER BY clauses
-- 5. Columns used in GROUP BY clauses
