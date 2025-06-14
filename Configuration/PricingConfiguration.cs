namespace PosBackend.Configuration
{
    public class PricingConfiguration
    {
        public const string SectionName = "Pricing";
        
        public string DefaultCurrency { get; set; } = "USD";
        public string[] SupportedCurrencies { get; set; } = { "USD", "EUR", "GBP", "ZAR", "CAD", "AUD", "JPY" };
        public TimeSpan CacheExchangeRatesFor { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan CachePricesFor { get; set; } = TimeSpan.FromMinutes(30);
        public bool EnableRealTimeCurrencyConversion { get; set; } = true;
        public bool EnableGeolocationCurrencyDetection { get; set; } = true;
        public ExchangeRateProviderSettings ExchangeRateProvider { get; set; } = new();
    }

    public class ExchangeRateProviderSettings
    {
        public string Provider { get; set; } = "exchangerate-api";
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.exchangerate-api.com/v4/latest/";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public int MaxRetries { get; set; } = 3;
        public bool UseFallbackRates { get; set; } = true;
    }
}