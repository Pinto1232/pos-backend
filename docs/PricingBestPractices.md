# Pricing System Best Practices Implementation

This document outlines the new pricing system implementation that follows industry best practices for multi-currency pricing, real-time exchange rates, and user experience.

## 🚀 Overview

The new pricing system replaces the previous JSON-based multi-currency approach with a proper relational database structure, real-time currency conversion, and intelligent currency detection.

## 📊 Key Improvements

### 1. **Proper Data Structure**
- **Before**: JSON field `MultiCurrencyPrices` in `PricingPackages` table
- **After**: Dedicated `PackagePrices` table with proper relationships

### 2. **Real-time Exchange Rates**
- **Before**: Hardcoded exchange rates in migrations
- **After**: Dynamic exchange rates from external APIs with caching

### 3. **Intelligent Currency Detection**
- **Before**: Simple IP-based detection
- **After**: Multi-factor detection (user preference → query params → headers → language → geo-location → default)

### 4. **Scalable Architecture**
- **Before**: Mixed responsibilities in controllers
- **After**: Dedicated services with clear separation of concerns

## 🗃️ Database Schema

### PackagePrices Table
```sql
CREATE TABLE "PackagePrices" (
    "Id" SERIAL PRIMARY KEY,
    "PackageId" INTEGER NOT NULL REFERENCES "PricingPackages"("Id"),
    "Currency" VARCHAR(3) NOT NULL,
    "Price" DECIMAL(18,2) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ValidUntil" TIMESTAMP WITH TIME ZONE NULL
);
```

### ExchangeRates Table
```sql
CREATE TABLE "ExchangeRates" (
    "Id" SERIAL PRIMARY KEY,
    "FromCurrency" VARCHAR(3) NOT NULL,
    "ToCurrency" VARCHAR(3) NOT NULL,
    "Rate" DECIMAL NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ExpiresAt" TIMESTAMP WITH TIME ZONE NULL
);
```

### Enhanced Currencies Table
```sql
ALTER TABLE "Currencies" ADD COLUMN "Name" VARCHAR(100) NOT NULL;
ALTER TABLE "Currencies" ADD COLUMN "Symbol" VARCHAR(10) NOT NULL;
ALTER TABLE "Currencies" ADD COLUMN "IsActive" BOOLEAN NOT NULL DEFAULT true;
ALTER TABLE "Currencies" ADD COLUMN "DecimalPlaces" INTEGER NOT NULL DEFAULT 2;
ALTER TABLE "Currencies" ADD COLUMN "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL;
```

## 🔧 Services Architecture

### ICurrencyService
Handles all currency-related operations:
- Real-time exchange rate fetching
- Currency conversion with caching
- Supported currency management
- Fallback rate handling

### ICurrencyDetectionService
Intelligent currency detection:
1. **User Preference** (authenticated users)
2. **Query Parameter** (`?currency=EUR`)
3. **HTTP Header** (`X-Currency: GBP`)
4. **Accept-Language** parsing
5. **IP Geolocation** (fallback)
6. **Default Currency** (USD)

### IPricingService
Package pricing operations:
- Multi-currency package pricing
- Custom package price calculation
- Price management for admins
- Caching for performance

## 🌐 API Endpoints

### New V2 API (Best Practices)
```
GET /api/v2/pricing-packages          # Auto-detects currency
GET /api/v2/pricing-packages?currency=EUR  # Explicit currency
GET /api/v2/pricing-packages/{id}     # Single package with pricing
GET /api/v2/pricing-packages/{id}/prices   # All prices for package
POST /api/v2/pricing-packages/{id}/prices  # Set price (Admin)
POST /api/v2/pricing-packages/convert      # Convert between currencies
GET /api/v2/pricing-packages/currencies    # Supported currencies
GET /api/v2/pricing-packages/exchange-rates/{base}  # Current rates
POST /api/v2/pricing-packages/user/currency-preference  # Set preference
```

### Legacy V1 API (Deprecated)
```
GET /api/pricing-packages  # Still works, shows deprecation warnings
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "Pricing": {
    "DefaultCurrency": "USD",
    "SupportedCurrencies": ["USD", "EUR", "GBP", "ZAR", "CAD", "AUD", "JPY"],
    "CacheExchangeRatesFor": "01:00:00",
    "CachePricesFor": "00:30:00",
    "EnableRealTimeCurrencyConversion": true,
    "EnableGeolocationCurrencyDetection": true,
    "ExchangeRateProvider": {
      "Provider": "exchangerate-api",
      "ApiKey": "your-api-key-here",
      "BaseUrl": "https://api.exchangerate-api.com/v4/latest/",
      "Timeout": "00:00:30",
      "MaxRetries": 3,
      "UseFallbackRates": true
    }
  }
}
```

## 🚦 Migration Guide

### 1. Apply Database Migration
```bash
dotnet ef database update
```

### 2. Run Data Migration Script
```bash
psql -d your_database -f Scripts/MigratePricingData.sql
```

### 3. Update Client Code
Replace old API calls with new V2 endpoints:

**Before:**
```javascript
fetch('/api/pricing-packages')
```

**After:**
```javascript
fetch('/api/v2/pricing-packages')  # Auto-detects currency
# OR
fetch('/api/v2/pricing-packages?currency=EUR')  # Explicit currency
```

### 4. Handle Currency Selection
```javascript
// Set user preference
await fetch('/api/v2/pricing-packages/user/currency-preference', {
  method: 'POST',
  body: JSON.stringify({ currency: 'EUR' })
});

// Get supported currencies
const currencies = await fetch('/api/v2/pricing-packages/currencies');
```

## 📈 Performance Optimizations

### Caching Strategy
- **Exchange Rates**: 1 hour cache with Redis fallback
- **Package Prices**: 30 minutes cache
- **Currency List**: 24 hours cache
- **User Preferences**: 30 days cache

### Database Indexes
```sql
CREATE INDEX "IX_PackagePrices_PackageId_Currency" ON "PackagePrices" ("PackageId", "Currency");
CREATE INDEX "IX_ExchangeRates_FromCurrency_ToCurrency" ON "ExchangeRates" ("FromCurrency", "ToCurrency");
```

## 🔒 Security Considerations

1. **Rate Limiting**: API calls to external exchange rate services
2. **Input Validation**: Currency codes must be 3-letter ISO codes
3. **Authorization**: Only admins can set package prices
4. **Caching**: Sensitive rate data has appropriate expiration

## 🧪 Testing

### Unit Tests
```bash
dotnet test --filter "Category=Pricing"
```

### Integration Tests
```bash
dotnet test --filter "Category=Integration&Category=Pricing"
```

### Test Currency Detection
```bash
curl -H "Accept-Language: en-GB,en;q=0.9" http://localhost:5000/api/v2/pricing-packages
curl -H "X-Currency: EUR" http://localhost:5000/api/v2/pricing-packages
curl "http://localhost:5000/api/v2/pricing-packages?currency=JPY"
```

## 📊 Monitoring

### Key Metrics
- Currency detection accuracy
- Exchange rate fetch success rate
- Cache hit ratios
- API response times by currency

### Logs to Monitor
- Currency detection decisions
- Exchange rate fetch failures
- Cache miss ratios
- Conversion errors

## 🔄 Rollback Plan

If issues arise, you can temporarily:

1. **Disable New Features**:
```json
{
  "Pricing": {
    "EnableRealTimeCurrencyConversion": false,
    "EnableGeolocationCurrencyDetection": false
  }
}
```

2. **Use Legacy Endpoints**: Old API still works with deprecation warnings

3. **Restore Database**: Migration can be reverted with `dotnet ef migrations remove`

## 📚 Further Reading

- [Exchange Rate API Documentation](https://exchangerate-api.com/docs)
- [Multi-Currency E-commerce Best Practices](https://stripe.com/docs/currencies)
- [Currency Detection Strategies](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language)
- [Redis Caching Patterns](https://redis.io/docs/manual/patterns/)

## 🤝 Contributing

When adding new currencies or features:

1. Update `SupportedCurrencies` in configuration
2. Add currency metadata to `Currencies` table
3. Update fallback rates in `CurrencyService`
4. Add appropriate tests
5. Update documentation

---

**Status**: ✅ **Implemented and Ready for Use**

**Migration Date**: December 2024  
**Next Review**: March 2025# Pricing System Best Practices Implementation

This document outlines the new pricing system implementation that follows industry best practices for multi-currency pricing, real-time exchange rates, and user experience.

## 🚀 Overview

The new pricing system replaces the previous JSON-based multi-currency approach with a proper relational database structure, real-time currency conversion, and intelligent currency detection.

## 📊 Key Improvements

### 1. **Proper Data Structure**
- **Before**: JSON field `MultiCurrencyPrices` in `PricingPackages` table
- **After**: Dedicated `PackagePrices` table with proper relationships

### 2. **Real-time Exchange Rates**
- **Before**: Hardcoded exchange rates in migrations
- **After**: Dynamic exchange rates from external APIs with caching

### 3. **Intelligent Currency Detection**
- **Before**: Simple IP-based detection
- **After**: Multi-factor detection (user preference → query params → headers → language → geo-location → default)

### 4. **Scalable Architecture**
- **Before**: Mixed responsibilities in controllers
- **After**: Dedicated services with clear separation of concerns

## 🗃️ Database Schema

### PackagePrices Table
```sql
CREATE TABLE "PackagePrices" (
    "Id" SERIAL PRIMARY KEY,
    "PackageId" INTEGER NOT NULL REFERENCES "PricingPackages"("Id"),
    "Currency" VARCHAR(3) NOT NULL,
    "Price" DECIMAL(18,2) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ValidUntil" TIMESTAMP WITH TIME ZONE NULL
);
```

### ExchangeRates Table
```sql
CREATE TABLE "ExchangeRates" (
    "Id" SERIAL PRIMARY KEY,
    "FromCurrency" VARCHAR(3) NOT NULL,
    "ToCurrency" VARCHAR(3) NOT NULL,
    "Rate" DECIMAL NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ExpiresAt" TIMESTAMP WITH TIME ZONE NULL
);
```

### Enhanced Currencies Table
```sql
ALTER TABLE "Currencies" ADD COLUMN "Name" VARCHAR(100) NOT NULL;
ALTER TABLE "Currencies" ADD COLUMN "Symbol" VARCHAR(10) NOT NULL;
ALTER TABLE "Currencies" ADD COLUMN "IsActive" BOOLEAN NOT NULL DEFAULT true;
ALTER TABLE "Currencies" ADD COLUMN "DecimalPlaces" INTEGER NOT NULL DEFAULT 2;
ALTER TABLE "Currencies" ADD COLUMN "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL;
```

## 🔧 Services Architecture

### ICurrencyService
Handles all currency-related operations:
- Real-time exchange rate fetching
- Currency conversion with caching
- Supported currency management
- Fallback rate handling

### ICurrencyDetectionService
Intelligent currency detection:
1. **User Preference** (authenticated users)
2. **Query Parameter** (`?currency=EUR`)
3. **HTTP Header** (`X-Currency: GBP`)
4. **Accept-Language** parsing
5. **IP Geolocation** (fallback)
6. **Default Currency** (USD)

### IPricingService
Package pricing operations:
- Multi-currency package pricing
- Custom package price calculation
- Price management for admins
- Caching for performance

## 🌐 API Endpoints

### New V2 API (Best Practices)
```
GET /api/v2/pricing-packages          # Auto-detects currency
GET /api/v2/pricing-packages?currency=EUR  # Explicit currency
GET /api/v2/pricing-packages/{id}     # Single package with pricing
GET /api/v2/pricing-packages/{id}/prices   # All prices for package
POST /api/v2/pricing-packages/{id}/prices  # Set price (Admin)
POST /api/v2/pricing-packages/convert      # Convert between currencies
GET /api/v2/pricing-packages/currencies    # Supported currencies
GET /api/v2/pricing-packages/exchange-rates/{base}  # Current rates
POST /api/v2/pricing-packages/user/currency-preference  # Set preference
```

### Legacy V1 API (Deprecated)
```
GET /api/pricing-packages  # Still works, shows deprecation warnings
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "Pricing": {
    "DefaultCurrency": "USD",
    "SupportedCurrencies": ["USD", "EUR", "GBP", "ZAR", "CAD", "AUD", "JPY"],
    "CacheExchangeRatesFor": "01:00:00",
    "CachePricesFor": "00:30:00",
    "EnableRealTimeCurrencyConversion": true,
    "EnableGeolocationCurrencyDetection": true,
    "ExchangeRateProvider": {
      "Provider": "exchangerate-api",
      "ApiKey": "your-api-key-here",
      "BaseUrl": "https://api.exchangerate-api.com/v4/latest/",
      "Timeout": "00:00:30",
      "MaxRetries": 3,
      "UseFallbackRates": true
    }
  }
}
```

## 🚦 Migration Guide

### 1. Apply Database Migration
```bash
dotnet ef database update
```

### 2. Run Data Migration Script
```bash
psql -d your_database -f Scripts/MigratePricingData.sql
```

### 3. Update Client Code
Replace old API calls with new V2 endpoints:

**Before:**
```javascript
fetch('/api/pricing-packages')
```

**After:**
```javascript
fetch('/api/v2/pricing-packages')  # Auto-detects currency
# OR
fetch('/api/v2/pricing-packages?currency=EUR')  # Explicit currency
```

### 4. Handle Currency Selection
```javascript
// Set user preference
await fetch('/api/v2/pricing-packages/user/currency-preference', {
  method: 'POST',
  body: JSON.stringify({ currency: 'EUR' })
});

// Get supported currencies
const currencies = await fetch('/api/v2/pricing-packages/currencies');
```

## 📈 Performance Optimizations

### Caching Strategy
- **Exchange Rates**: 1 hour cache with Redis fallback
- **Package Prices**: 30 minutes cache
- **Currency List**: 24 hours cache
- **User Preferences**: 30 days cache

### Database Indexes
```sql
CREATE INDEX "IX_PackagePrices_PackageId_Currency" ON "PackagePrices" ("PackageId", "Currency");
CREATE INDEX "IX_ExchangeRates_FromCurrency_ToCurrency" ON "ExchangeRates" ("FromCurrency", "ToCurrency");
```

## 🔒 Security Considerations

1. **Rate Limiting**: API calls to external exchange rate services
2. **Input Validation**: Currency codes must be 3-letter ISO codes
3. **Authorization**: Only admins can set package prices
4. **Caching**: Sensitive rate data has appropriate expiration

## 🧪 Testing

### Unit Tests
```bash
dotnet test --filter "Category=Pricing"
```

### Integration Tests
```bash
dotnet test --filter "Category=Integration&Category=Pricing"
```

### Test Currency Detection
```bash
curl -H "Accept-Language: en-GB,en;q=0.9" http://localhost:5000/api/v2/pricing-packages
curl -H "X-Currency: EUR" http://localhost:5000/api/v2/pricing-packages
curl "http://localhost:5000/api/v2/pricing-packages?currency=JPY"
```

## 📊 Monitoring

### Key Metrics
- Currency detection accuracy
- Exchange rate fetch success rate
- Cache hit ratios
- API response times by currency

### Logs to Monitor
- Currency detection decisions
- Exchange rate fetch failures
- Cache miss ratios
- Conversion errors

## 🔄 Rollback Plan

If issues arise, you can temporarily:

1. **Disable New Features**:
```json
{
  "Pricing": {
    "EnableRealTimeCurrencyConversion": false,
    "EnableGeolocationCurrencyDetection": false
  }
}
```

2. **Use Legacy Endpoints**: Old API still works with deprecation warnings

3. **Restore Database**: Migration can be reverted with `dotnet ef migrations remove`

## 📚 Further Reading

- [Exchange Rate API Documentation](https://exchangerate-api.com/docs)
- [Multi-Currency E-commerce Best Practices](https://stripe.com/docs/currencies)
- [Currency Detection Strategies](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language)
- [Redis Caching Patterns](https://redis.io/docs/manual/patterns/)

## 🤝 Contributing

When adding new currencies or features:

1. Update `SupportedCurrencies` in configuration
2. Add currency metadata to `Currencies` table
3. Update fallback rates in `CurrencyService`
4. Add appropriate tests
5. Update documentation

---

**Status**: ✅ **Implemented and Ready for Use**

**Migration Date**: December 2024  
**Next Review**: March 2025