# Proactive Payment Failure Prevention System

## Overview

This system implements comprehensive proactive payment failure prevention to reduce subscription churn by addressing potential payment issues before they occur. The system monitors card expirations, sends payment reminders, implements smart retry logic, and provides detailed payment health insights.

## Features

### 1. Card Expiration Monitoring
- **Automatic Detection**: Monitors all payment methods for upcoming expirations
- **Multi-Stage Warnings**: Sends notifications at 30, 7, and 1 day(s) before expiration
- **Email Notifications**: Professional HTML email templates with clear call-to-action
- **Tracking**: Prevents duplicate notifications and tracks warning status

### 2. Upcoming Payment Reminders
- **Billing Reminders**: Notifies users 7 and 1 day(s) before subscription renewal
- **Amount Display**: Shows exact billing amount and date
- **Payment Method Verification**: Encourages users to verify payment information
- **Customizable Schedule**: Configurable reminder timing via appsettings.json

### 3. Smart Payment Retry Logic
- **Exponential Backoff**: Intelligent retry intervals (1h, 6h, 24h, 72h)
- **Failure Tracking**: Comprehensive logging of failure reasons and attempts
- **Success Notifications**: Confirms when retry payments succeed
- **Manual Retry**: Admin capability to trigger manual payment retries

### 4. Payment Health Monitoring
- **Risk Assessment**: Categorizes subscriptions by payment risk level
- **Health Dashboard**: Comprehensive view of payment status and issues
- **Proactive Alerts**: Early warning system for potential problems
- **Historical Tracking**: Complete audit trail of payment events

## Architecture

### Core Services

#### EmailService
- **Purpose**: Handles all email communications
- **Features**: HTML templates, SMTP configuration, delivery tracking
- **Templates**: Card expiration, payment reminders, failure notifications, success confirmations

#### PaymentMonitoringService
- **Purpose**: Core monitoring and processing logic
- **Functions**: Card monitoring, payment reminders, retry processing, Stripe synchronization
- **Integration**: Direct Stripe API integration for real-time data

#### PaymentMonitoringBackgroundService
- **Purpose**: Automated background processing
- **Schedule**: Configurable interval (default: 60 minutes)
- **Tasks**: Runs all monitoring functions automatically
- **Error Handling**: Robust error handling with retry logic

### Data Models

#### PaymentMethodInfo
- Stores payment method details and expiration tracking
- Tracks warning notification status
- Provides computed properties for expiration status

#### PaymentNotificationHistory
- Complete audit trail of all notifications sent
- Tracks delivery status and retry attempts
- Stores contextual data for analysis

#### PaymentRetryAttempt
- Detailed tracking of payment retry attempts
- Configurable retry strategies
- Links to Stripe events and invoices

### Database Schema

```sql
-- Key tables for payment monitoring
PaymentMethodInfos          -- Payment method tracking
PaymentNotificationHistories -- Notification audit trail
PaymentRetryAttempts        -- Retry attempt tracking
PaymentHealthView           -- Aggregated health metrics
```

## Configuration

### appsettings.json

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "notifications@pisvaltech.com",
    "SmtpPassword": "your_app_password_here",
    "FromEmail": "no-reply@pisvaltech.com",
    "FromName": "PisVal POS System",
    "EnableSsl": true,
    "EnableEmailNotifications": true
  },
  "PaymentMonitoring": {
    "CardExpirationWarningDays": [30, 7, 1],
    "UpcomingPaymentReminderDays": [7, 1],
    "FailedPaymentRetryIntervalHours": [1, 6, 24, 72],
    "MaxRetryAttempts": 4,
    "EnableProactiveMonitoring": true,
    "MonitoringIntervalMinutes": 60
  }
}
```

## API Endpoints

### Payment Monitoring Controller

```
GET    /api/PaymentMonitoring/payment-methods/{userId}
GET    /api/PaymentMonitoring/default-payment-method/{userId}
POST   /api/PaymentMonitoring/sync-payment-methods/{userId}
GET    /api/PaymentMonitoring/notification-history/{userId}
GET    /api/PaymentMonitoring/retry-attempts/{userId}
GET    /api/PaymentMonitoring/payment-health/{userId}
POST   /api/PaymentMonitoring/manual-retry/{retryAttemptId}
POST   /api/PaymentMonitoring/run-monitoring [Admin Only]
```

### Webhook Integration

Enhanced Stripe webhook handlers for:
- `invoice.payment_failed` - Creates retry attempts and sends notifications
- `invoice.payment_succeeded` - Resets failure counters and confirms success
- `payment_method.attached/detached/updated` - Syncs payment method data
- `customer.subscription.updated/deleted` - Updates subscription status

## Email Templates

### Card Expiration Warning
- **Trigger**: 30, 7, 1 day(s) before expiration
- **Content**: Urgency-based messaging, clear update instructions
- **CTA**: Direct link to payment method update page

### Upcoming Payment Reminder
- **Trigger**: 7, 1 day(s) before billing
- **Content**: Amount, date, payment method verification
- **CTA**: View billing details and update payment method

### Payment Failed Notification
- **Trigger**: Immediate after payment failure
- **Content**: Failure reason, next retry date, resolution steps
- **CTA**: Update payment method immediately

### Payment Success Confirmation
- **Trigger**: After successful retry payment
- **Content**: Confirmation of payment, amount, date
- **CTA**: View receipt and account details

## Monitoring and Analytics

### Payment Health View
SQL view providing aggregated metrics:
- Subscription status and billing dates
- Failure attempt counts and reasons
- Payment method expiration status
- Risk level assessment

### Risk Level Calculation
- **High Risk**: 3+ failed payment attempts
- **Medium Risk**: 1-2 failed attempts OR card expiring within 7 days
- **Low Risk**: Card expiring within 30 days
- **Healthy**: No issues detected

## Implementation Benefits

### Churn Reduction
- **Proactive Approach**: Address issues before they cause cancellations
- **User Experience**: Clear communication and easy resolution paths
- **Automation**: Reduces manual intervention and support tickets

### Business Intelligence
- **Payment Patterns**: Understand common failure reasons
- **Success Metrics**: Track prevention effectiveness
- **Customer Insights**: Identify at-risk customer segments

### Operational Efficiency
- **Automated Monitoring**: Reduces manual oversight requirements
- **Comprehensive Logging**: Full audit trail for troubleshooting
- **Scalable Architecture**: Handles growing subscriber base

## Deployment Checklist

1. **Database Migration**: Run `AddPaymentMonitoringTables.sql`
2. **Configuration**: Update appsettings.json with email and monitoring settings
3. **Stripe Webhooks**: Configure additional webhook events
4. **Email Templates**: Customize email templates for brand consistency
5. **Testing**: Verify email delivery and monitoring functionality
6. **Monitoring**: Set up alerts for background service health

## Future Enhancements

### Phase 2 Features
- SMS notifications for critical alerts
- In-app notification system
- Customer self-service payment update portal
- Advanced analytics dashboard

### Phase 3 Features
- Machine learning for failure prediction
- Dynamic retry strategies based on failure patterns
- Integration with customer support systems
- Multi-currency payment monitoring

## Support and Maintenance

### Monitoring
- Background service health checks
- Email delivery rate monitoring
- Payment success/failure rate tracking
- System performance metrics

### Troubleshooting
- Comprehensive logging at all levels
- Error tracking and alerting
- Manual override capabilities
- Detailed audit trails

This proactive payment failure prevention system significantly reduces subscription churn by addressing payment issues before they result in service interruptions, providing a better customer experience and improved business outcomes.
