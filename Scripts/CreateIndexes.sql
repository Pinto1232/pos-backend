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

-- Note: This script has been customized based on the tables that were confirmed to exist
-- in your database. You can add more indexes as you identify performance bottlenecks.
--
-- Common patterns to index:
-- 1. Foreign keys (columns ending with 'Id')
-- 2. Columns used in WHERE clauses
-- 3. Columns used in JOIN conditions
-- 4. Columns used in ORDER BY clauses
-- 5. Columns used in GROUP BY clauses
