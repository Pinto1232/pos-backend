-- Migration script to add subscription fields to existing tables
-- Run this script after creating the migration with: dotnet ef migrations add AddSubscriptionFields

-- Add Stripe subscription fields to UserSubscription table
ALTER TABLE "UserSubscriptions"
ADD COLUMN "StripeSubscriptionId" TEXT,
ADD COLUMN "StripeCustomerId" TEXT,
ADD COLUMN "StripePriceId" TEXT,
ADD COLUMN "Status" VARCHAR(50) DEFAULT 'active',
ADD COLUMN "TrialStart" TIMESTAMP,
ADD COLUMN "TrialEnd" TIMESTAMP,
ADD COLUMN "CurrentPeriodStart" TIMESTAMP,
ADD COLUMN "CurrentPeriodEnd" TIMESTAMP,
ADD COLUMN "CancelAtPeriodEnd" BOOLEAN DEFAULT FALSE,
ADD COLUMN "CanceledAt" TIMESTAMP,
ADD COLUMN "LastPaymentAmount" DECIMAL(18,2),
ADD COLUMN "LastPaymentDate" TIMESTAMP,
ADD COLUMN "NextBillingDate" TIMESTAMP,
ADD COLUMN "Currency" VARCHAR(10) DEFAULT 'USD';

-- Add Stripe integration fields to PricingPackages table
ALTER TABLE "PricingPackages"
ADD COLUMN "StripeProductId" TEXT,
ADD COLUMN "StripePriceId" TEXT,
ADD COLUMN "StripeMultiCurrencyPriceIds" TEXT DEFAULT '{}',
ADD COLUMN "IsSubscription" BOOLEAN DEFAULT TRUE,
ADD COLUMN "BillingInterval" VARCHAR(20) DEFAULT 'month',
ADD COLUMN "BillingIntervalCount" INTEGER DEFAULT 1;

-- Create StripeSubscriptions table
CREATE TABLE "StripeSubscriptions" (
    "Id" SERIAL PRIMARY KEY,
    "StripeSubscriptionId" TEXT NOT NULL,
    "StripeCustomerId" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "UserSubscriptionId" INTEGER NOT NULL,
    "StripePriceId" TEXT NOT NULL,
    "StripeProductId" TEXT NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "TrialStart" TIMESTAMP,
    "TrialEnd" TIMESTAMP,
    "CurrentPeriodStart" TIMESTAMP,
    "CurrentPeriodEnd" TIMESTAMP,
    "CancelAtPeriodEnd" BOOLEAN DEFAULT FALSE,
    "CanceledAt" TIMESTAMP,
    "EndedAt" TIMESTAMP,
    "LatestInvoiceId" TEXT,
    "LatestInvoiceAmount" DECIMAL(18,2),
    "LatestInvoiceStatus" TEXT,
    "LatestInvoiceDate" TIMESTAMP,
    "DefaultPaymentMethodId" TEXT,
    "PaymentMethodType" TEXT,
    "PaymentMethodLast4" TEXT,
    "PaymentMethodBrand" TEXT,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Currency" VARCHAR(10) DEFAULT 'USD',
    "BillingInterval" VARCHAR(20) DEFAULT 'month',
    "BillingIntervalCount" INTEGER DEFAULT 1,
    "Metadata" TEXT DEFAULT '{}',
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "LastUpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "LastSyncedAt" TIMESTAMP,
    "LastWebhookEventId" TEXT,
    "LastWebhookEventDate" TIMESTAMP,
    "FailedPaymentAttempts" INTEGER DEFAULT 0,
    "LastFailedPaymentDate" TIMESTAMP,
    "LastFailureReason" TEXT,
    "CouponId" TEXT,
    "DiscountAmount" DECIMAL(18,2),
    "DiscountPercentage" DECIMAL(18,2),
    "DiscountStart" TIMESTAMP,
    "DiscountEnd" TIMESTAMP,

    CONSTRAINT "FK_StripeSubscriptions_UserSubscriptions_UserSubscriptionId"
        FOREIGN KEY ("UserSubscriptionId") REFERENCES "UserSubscriptions" ("Id") ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX "IX_UserSubscriptions_UserId" ON "UserSubscriptions" ("UserId");
CREATE INDEX "IX_UserSubscriptions_StripeSubscriptionId" ON "UserSubscriptions" ("StripeSubscriptionId");
CREATE INDEX "IX_UserSubscriptions_StripeCustomerId" ON "UserSubscriptions" ("StripeCustomerId");
CREATE INDEX "IX_UserSubscriptions_Status" ON "UserSubscriptions" ("Status");
CREATE INDEX "IX_UserSubscriptions_IsActive" ON "UserSubscriptions" ("IsActive");

CREATE INDEX "IX_StripeSubscriptions_StripeSubscriptionId" ON "StripeSubscriptions" ("StripeSubscriptionId");
CREATE INDEX "IX_StripeSubscriptions_StripeCustomerId" ON "StripeSubscriptions" ("StripeCustomerId");
CREATE INDEX "IX_StripeSubscriptions_UserId" ON "StripeSubscriptions" ("UserId");
CREATE INDEX "IX_StripeSubscriptions_Status" ON "StripeSubscriptions" ("Status");
CREATE INDEX "IX_StripeSubscriptions_UserSubscriptionId" ON "StripeSubscriptions" ("UserSubscriptionId");

CREATE INDEX "IX_PricingPackages_StripeProductId" ON "PricingPackages" ("StripeProductId");
CREATE INDEX "IX_PricingPackages_StripePriceId" ON "PricingPackages" ("StripePriceId");
CREATE INDEX "IX_PricingPackages_Type" ON "PricingPackages" ("Type");
CREATE INDEX "IX_PricingPackages_IsSubscription" ON "PricingPackages" ("IsSubscription");

-- Update existing pricing packages with Stripe product and price IDs
-- Note: Replace these with actual Stripe IDs after creating products in Stripe dashboard

-- Update existing packages with proper billing information for old package types
UPDATE "PricingPackages"
SET
    "IsSubscription" = TRUE,
    "BillingInterval" = 'month',
    "BillingIntervalCount" = 1,
    "StripeMultiCurrencyPriceIds" = '{}'
WHERE "Type" IN ('starter', 'growth', 'premium', 'enterprise', 'custom');

UPDATE "PricingPackages"
SET
    "StripeProductId" = 'prod_starter_plus',
    "StripePriceId" = 'price_starter_plus_usd_monthly',
    "StripeMultiCurrencyPriceIds" = '{"USD": "price_starter_plus_usd_monthly", "EUR": "price_starter_plus_eur_monthly", "GBP": "price_starter_plus_gbp_monthly", "ZAR": "price_starter_plus_zar_monthly"}',
    "IsSubscription" = TRUE,
    "BillingInterval" = 'month',
    "BillingIntervalCount" = 1
WHERE "Type" = 'starter-plus';

UPDATE "PricingPackages"
SET
    "StripeProductId" = 'prod_growth_pro',
    "StripePriceId" = 'price_growth_pro_usd_monthly',
    "StripeMultiCurrencyPriceIds" = '{"USD": "price_growth_pro_usd_monthly", "EUR": "price_growth_pro_eur_monthly", "GBP": "price_growth_pro_gbp_monthly", "ZAR": "price_growth_pro_zar_monthly"}',
    "IsSubscription" = TRUE,
    "BillingInterval" = 'month',
    "BillingIntervalCount" = 1
WHERE "Type" = 'growth-pro';

UPDATE "PricingPackages"
SET
    "StripeProductId" = 'prod_custom_pro',
    "StripePriceId" = 'price_custom_pro_usd_monthly',
    "StripeMultiCurrencyPriceIds" = '{"USD": "price_custom_pro_usd_monthly", "EUR": "price_custom_pro_eur_monthly", "GBP": "price_custom_pro_gbp_monthly", "ZAR": "price_custom_pro_zar_monthly"}',
    "IsSubscription" = TRUE,
    "BillingInterval" = 'month',
    "BillingIntervalCount" = 1
WHERE "Type" = 'custom-pro';

UPDATE "PricingPackages"
SET
    "StripeProductId" = 'prod_enterprise_elite',
    "StripePriceId" = 'price_enterprise_elite_usd_monthly',
    "StripeMultiCurrencyPriceIds" = '{"USD": "price_enterprise_elite_usd_monthly", "EUR": "price_enterprise_elite_eur_monthly", "GBP": "price_enterprise_elite_gbp_monthly", "ZAR": "price_enterprise_elite_zar_monthly"}',
    "IsSubscription" = TRUE,
    "BillingInterval" = 'month',
    "BillingIntervalCount" = 1
WHERE "Type" = 'enterprise-elite';

UPDATE "PricingPackages"
SET
    "StripeProductId" = 'prod_premium_plus',
    "StripePriceId" = 'price_premium_plus_usd_monthly',
    "StripeMultiCurrencyPriceIds" = '{"USD": "price_premium_plus_usd_monthly", "EUR": "price_premium_plus_eur_monthly", "GBP": "price_premium_plus_gbp_monthly", "ZAR": "price_premium_plus_zar_monthly"}',
    "IsSubscription" = TRUE,
    "BillingInterval" = 'month',
    "BillingIntervalCount" = 1
WHERE "Type" = 'premium-plus';

-- Add constraints
ALTER TABLE "StripeSubscriptions"
ADD CONSTRAINT "UQ_StripeSubscriptions_StripeSubscriptionId" UNIQUE ("StripeSubscriptionId");

-- Add check constraints
ALTER TABLE "UserSubscriptions"
ADD CONSTRAINT "CK_UserSubscriptions_Status"
CHECK ("Status" IN ('active', 'trialing', 'past_due', 'canceled', 'unpaid', 'incomplete', 'incomplete_expired'));

ALTER TABLE "StripeSubscriptions"
ADD CONSTRAINT "CK_StripeSubscriptions_Status"
CHECK ("Status" IN ('active', 'trialing', 'past_due', 'canceled', 'unpaid', 'incomplete', 'incomplete_expired'));

ALTER TABLE "PricingPackages"
ADD CONSTRAINT "CK_PricingPackages_BillingInterval"
CHECK ("BillingInterval" IN ('day', 'week', 'month', 'year'));

-- Create trigger to update LastUpdatedAt timestamp
CREATE OR REPLACE FUNCTION update_last_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW."LastUpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_user_subscriptions_last_updated_at
    BEFORE UPDATE ON "UserSubscriptions"
    FOR EACH ROW
    EXECUTE FUNCTION update_last_updated_at();

CREATE TRIGGER trigger_update_stripe_subscriptions_last_updated_at
    BEFORE UPDATE ON "StripeSubscriptions"
    FOR EACH ROW
    EXECUTE FUNCTION update_last_updated_at();

-- Add comments for documentation
COMMENT ON TABLE "StripeSubscriptions" IS 'Stores Stripe-specific subscription data for synchronization with Stripe webhooks';
COMMENT ON COLUMN "UserSubscriptions"."StripeSubscriptionId" IS 'Stripe subscription ID for API calls';
COMMENT ON COLUMN "UserSubscriptions"."Status" IS 'Subscription status: active, trialing, past_due, canceled, unpaid, incomplete, incomplete_expired';
COMMENT ON COLUMN "PricingPackages"."StripeProductId" IS 'Stripe product ID for subscription creation';
COMMENT ON COLUMN "PricingPackages"."StripePriceId" IS 'Default Stripe price ID (usually USD)';
COMMENT ON COLUMN "PricingPackages"."StripeMultiCurrencyPriceIds" IS 'JSON object containing price IDs for different currencies';
