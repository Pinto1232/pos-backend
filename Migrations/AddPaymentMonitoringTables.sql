-- Migration: Add Payment Monitoring Tables
-- Description: Creates tables for proactive payment failure prevention
-- Date: 2024-01-XX

-- Create PaymentMethodInfos table
CREATE TABLE IF NOT EXISTS "PaymentMethodInfos" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "StripeCustomerId" TEXT NOT NULL,
    "StripePaymentMethodId" TEXT NOT NULL,
    "Type" VARCHAR(20) NOT NULL DEFAULT '',
    "CardBrand" VARCHAR(20),
    "CardLast4" VARCHAR(4),
    "CardExpMonth" INTEGER,
    "CardExpYear" INTEGER,
    "CardCountry" VARCHAR(50),
    "CardFunding" VARCHAR(20),
    "IsDefault" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastUpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastUsedAt" TIMESTAMP,
    "ExpirationDate" TIMESTAMP,
    "ExpirationWarning30DaysSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "ExpirationWarning7DaysSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "ExpirationWarning1DaySent" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Create indexes for PaymentMethodInfos
CREATE INDEX IF NOT EXISTS "IX_PaymentMethodInfos_UserId" ON "PaymentMethodInfos" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_PaymentMethodInfos_StripeCustomerId" ON "PaymentMethodInfos" ("StripeCustomerId");
CREATE INDEX IF NOT EXISTS "IX_PaymentMethodInfos_StripePaymentMethodId" ON "PaymentMethodInfos" ("StripePaymentMethodId");
CREATE INDEX IF NOT EXISTS "IX_PaymentMethodInfos_ExpirationDate" ON "PaymentMethodInfos" ("ExpirationDate");
CREATE INDEX IF NOT EXISTS "IX_PaymentMethodInfos_IsActive" ON "PaymentMethodInfos" ("IsActive");

-- Create PaymentNotificationHistories table
CREATE TABLE IF NOT EXISTS "PaymentNotificationHistories" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "NotificationType" VARCHAR(100) NOT NULL,
    "Subject" VARCHAR(200) NOT NULL,
    "Message" TEXT NOT NULL,
    "DeliveryMethod" VARCHAR(100) NOT NULL,
    "Recipient" VARCHAR(200) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "SentAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DeliveredAt" TIMESTAMP,
    "ReadAt" TIMESTAMP,
    "StripeSubscriptionId" TEXT,
    "StripePaymentMethodId" TEXT,
    "StripeInvoiceId" TEXT,
    "ErrorMessage" TEXT,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "NextRetryAt" TIMESTAMP,
    "ContextData" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastUpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for PaymentNotificationHistories
CREATE INDEX IF NOT EXISTS "IX_PaymentNotificationHistories_UserId" ON "PaymentNotificationHistories" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_PaymentNotificationHistories_NotificationType" ON "PaymentNotificationHistories" ("NotificationType");
CREATE INDEX IF NOT EXISTS "IX_PaymentNotificationHistories_Status" ON "PaymentNotificationHistories" ("Status");
CREATE INDEX IF NOT EXISTS "IX_PaymentNotificationHistories_SentAt" ON "PaymentNotificationHistories" ("SentAt");
CREATE INDEX IF NOT EXISTS "IX_PaymentNotificationHistories_StripeSubscriptionId" ON "PaymentNotificationHistories" ("StripeSubscriptionId");

-- Create PaymentRetryAttempts table
CREATE TABLE IF NOT EXISTS "PaymentRetryAttempts" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "StripeSubscriptionId" TEXT NOT NULL,
    "StripeInvoiceId" TEXT NOT NULL,
    "StripePaymentIntentId" TEXT NOT NULL,
    "AttemptNumber" INTEGER NOT NULL DEFAULT 1,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Currency" VARCHAR(10) NOT NULL DEFAULT 'USD',
    "Status" VARCHAR(50) NOT NULL,
    "AttemptedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CompletedAt" TIMESTAMP,
    "NextRetryAt" TIMESTAMP,
    "FailureCode" TEXT,
    "FailureMessage" TEXT,
    "DeclineCode" TEXT,
    "RetryStrategy" VARCHAR(50) NOT NULL DEFAULT 'exponential_backoff',
    "RetryIntervalHours" INTEGER NOT NULL DEFAULT 1,
    "IsAutomaticRetry" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsManualRetry" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotificationSentAt" TIMESTAMP,
    "StripeEventId" TEXT,
    "AdditionalMetadata" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastUpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for PaymentRetryAttempts
CREATE INDEX IF NOT EXISTS "IX_PaymentRetryAttempts_UserId" ON "PaymentRetryAttempts" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_PaymentRetryAttempts_StripeSubscriptionId" ON "PaymentRetryAttempts" ("StripeSubscriptionId");
CREATE INDEX IF NOT EXISTS "IX_PaymentRetryAttempts_Status" ON "PaymentRetryAttempts" ("Status");
CREATE INDEX IF NOT EXISTS "IX_PaymentRetryAttempts_NextRetryAt" ON "PaymentRetryAttempts" ("NextRetryAt");
CREATE INDEX IF NOT EXISTS "IX_PaymentRetryAttempts_AttemptedAt" ON "PaymentRetryAttempts" ("AttemptedAt");

-- Add foreign key constraints
ALTER TABLE "PaymentRetryAttempts" 
ADD CONSTRAINT "FK_PaymentRetryAttempts_StripeSubscriptions_StripeSubscriptionId" 
FOREIGN KEY ("StripeSubscriptionId") REFERENCES "StripeSubscriptions" ("StripeSubscriptionId") 
ON DELETE CASCADE;

-- Update existing StripeSubscriptions table if it doesn't have the failure tracking fields
DO $$
BEGIN
    -- Check if FailedPaymentAttempts column exists, if not add it
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'StripeSubscriptions' 
                   AND column_name = 'FailedPaymentAttempts') THEN
        ALTER TABLE "StripeSubscriptions" ADD COLUMN "FailedPaymentAttempts" INTEGER NOT NULL DEFAULT 0;
    END IF;

    -- Check if LastFailedPaymentDate column exists, if not add it
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'StripeSubscriptions' 
                   AND column_name = 'LastFailedPaymentDate') THEN
        ALTER TABLE "StripeSubscriptions" ADD COLUMN "LastFailedPaymentDate" TIMESTAMP;
    END IF;

    -- Check if LastFailureReason column exists, if not add it
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                   WHERE table_name = 'StripeSubscriptions' 
                   AND column_name = 'LastFailureReason') THEN
        ALTER TABLE "StripeSubscriptions" ADD COLUMN "LastFailureReason" TEXT;
    END IF;
END $$;

-- Create a view for payment health monitoring
CREATE OR REPLACE VIEW "PaymentHealthView" AS
SELECT 
    u."UserId",
    u."StripeSubscriptionId",
    u."Status" as "SubscriptionStatus",
    u."NextBillingDate",
    s."FailedPaymentAttempts",
    s."LastFailedPaymentDate",
    s."LastFailureReason",
    pm."CardBrand",
    pm."CardLast4",
    pm."ExpirationDate",
    pm."IsExpired",
    CASE 
        WHEN pm."ExpirationDate" IS NOT NULL AND pm."ExpirationDate" <= CURRENT_TIMESTAMP + INTERVAL '30 days' 
        THEN TRUE 
        ELSE FALSE 
    END as "IsExpiringSoon",
    CASE 
        WHEN s."FailedPaymentAttempts" >= 3 THEN 'High Risk'
        WHEN s."FailedPaymentAttempts" >= 1 THEN 'Medium Risk'
        WHEN pm."ExpirationDate" IS NOT NULL AND pm."ExpirationDate" <= CURRENT_TIMESTAMP + INTERVAL '7 days' THEN 'Medium Risk'
        ELSE 'Low Risk'
    END as "RiskLevel"
FROM "UserSubscriptions" u
LEFT JOIN "StripeSubscriptions" s ON u."StripeSubscriptionId" = s."StripeSubscriptionId"
LEFT JOIN "PaymentMethodInfos" pm ON u."UserId" = pm."UserId" AND pm."IsDefault" = TRUE AND pm."IsActive" = TRUE
WHERE u."IsActive" = TRUE;

-- Insert initial configuration data
INSERT INTO "PaymentNotificationHistories" ("UserId", "NotificationType", "Subject", "Message", "DeliveryMethod", "Recipient", "Status", "SentAt")
VALUES ('system', 'SystemInitialization', 'Payment Monitoring System Initialized', 'Proactive payment failure prevention system has been successfully initialized.', 'System', 'system@pisvaltech.com', 'Sent', CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

COMMIT;
