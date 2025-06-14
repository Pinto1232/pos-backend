using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;
using PosBackend.Services.Interfaces;
using System.Text.Json;

namespace PosBackend.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly PosDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CurrencyService> _logger;
        private readonly PricingOptions _options;

        private readonly Dictionary<string, string> _currencyNames = new()
        {
            { "USD", "US Dollar" },
            { "EUR", "Euro" },
            { "GBP", "British Pound" },
            { "ZAR", "South African Rand" },
            { "CAD", "Canadian Dollar" },
            { "AUD", "Australian Dollar" },
            { "JPY", "Japanese Yen" }
        };

        private readonly Dictionary<string, string> _currencySymbols = new()
        {
            { "USD", "$" },
            { "EUR", "€" },
            { "GBP", "£" },
            { "ZAR", "R" },
            { "CAD", "C$" },
            { "AUD", "A$" },
            { "JPY", "¥" }
        };

        public CurrencyService(
            PosDbContext context,
            IHttpClientFactory httpClientFactory,
            ICacheService cacheService,
            ILogger<CurrencyService> logger,
            IOptions<PricingOptions> options)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return amount;

            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            return Math.Round(amount * rate, GetCurrencyDecimalPlaces(toCurrency));
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return 1.0m;

            var cacheKey = $"exchange_rate_{fromCurrency}_{toCurrency}";
            var cachedRate = await _cacheService.GetAsync<decimal?>(cacheKey);

            if (cachedRate.HasValue)
            {
                return cachedRate.Value;
            }

            // Try to get from database first
            var dbRate = await _context.ExchangeRates
                .Where(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency)
                .Where(r => r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (dbRate != null)
            {
                await _cacheService.SetAsync(cacheKey, dbRate.Rate, null, TimeSpan.FromMinutes(30));
                return dbRate.Rate;
            }

            // Fetch from external API
            var rate = await FetchExchangeRateFromApiAsync(fromCurrency, toCurrency);
            
            // Store in database
            var exchangeRate = new ExchangeRate
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rate,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _context.ExchangeRates.Add(exchangeRate);
            await _context.SaveChangesAsync();

            // Cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, rate, null, TimeSpan.FromMinutes(30));

            return rate;
        }

        public async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency)
        {
            var cacheKey = $"exchange_rates_{baseCurrency}";
            var cachedRates = await _cacheService.GetAsync<Dictionary<string, decimal>>(cacheKey);

            if (cachedRates != null)
            {
                return cachedRates;
            }

            var rates = new Dictionary<string, decimal>();
            var supportedCurrencies = _options.SupportedCurrencies ?? new[] { "USD", "EUR", "GBP", "ZAR" };

            foreach (var currency in supportedCurrencies)
            {
                if (currency != baseCurrency)
                {
                    rates[currency] = await GetExchangeRateAsync(baseCurrency, currency);
                }
            }

            await _cacheService.SetAsync(cacheKey, rates, null, TimeSpan.FromHours(1));
            return rates;
        }

        public async Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync()
        {
            var cacheKey = "supported_currencies";
            var cached = await _cacheService.GetAsync<List<Currency>>(cacheKey);

            if (cached != null)
            {
                return cached;
            }

            var currencies = await _context.Currencies
                .Where(c => c.IsActive)
                .ToListAsync();

            if (!currencies.Any())
            {
                // Seed default currencies if none exist
                await SeedDefaultCurrenciesAsync();
                currencies = await _context.Currencies.Where(c => c.IsActive).ToListAsync();
            }

            await _cacheService.SetAsync(cacheKey, currencies, null, TimeSpan.FromHours(24));
            return currencies;
        }

        public bool IsCurrencySupported(string currency)
        {
            var supportedCurrencies = _options.SupportedCurrencies ?? new[] { "USD", "EUR", "GBP", "ZAR" };
            return supportedCurrencies.Contains(currency);
        }

        public async Task<CurrencyConversionResult> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            var rate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            var convertedAmount = await ConvertAsync(amount, fromCurrency, toCurrency);

            return new CurrencyConversionResult
            {
                OriginalAmount = amount,
                FromCurrency = fromCurrency,
                ConvertedAmount = convertedAmount,
                ToCurrency = toCurrency,
                ExchangeRate = rate,
                ConvertedAt = DateTime.UtcNow
            };
        }

        public async Task RefreshExchangeRatesAsync()
        {
            try
            {
                _logger.LogInformation("Starting exchange rate refresh");
                
                var supportedCurrencies = _options.SupportedCurrencies ?? new[] { "USD", "EUR", "GBP", "ZAR" };
                var baseCurrency = _options.DefaultCurrency ?? "USD";

                foreach (var targetCurrency in supportedCurrencies)
                {
                    if (targetCurrency != baseCurrency)
                    {
                        await GetExchangeRateAsync(baseCurrency, targetCurrency);
                    }
                }

                _logger.LogInformation("Exchange rate refresh completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing exchange rates");
            }
        }

        private async Task<decimal> FetchExchangeRateFromApiAsync(string fromCurrency, string toCurrency)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.exchangerate-api.com/v4/latest/{fromCurrency}";
                
                var response = await client.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);
                
                if (data.TryGetProperty("rates", out var rates) && 
                    rates.TryGetProperty(toCurrency, out var rateElement))
                {
                    return rateElement.GetDecimal();
                }
                
                throw new InvalidOperationException($"Exchange rate not found for {fromCurrency} to {toCurrency}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate from API for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                
                // Fallback to hardcoded rates (temporary)
                return GetFallbackRate(fromCurrency, toCurrency);
            }
        }

        private decimal GetFallbackRate(string fromCurrency, string toCurrency)
        {
            // Hardcoded fallback rates (should be updated regularly)
            var fallbackRates = new Dictionary<string, Dictionary<string, decimal>>
            {
                ["USD"] = new() { ["EUR"] = 0.93m, ["GBP"] = 0.80m, ["ZAR"] = 18.5m, ["CAD"] = 1.35m, ["AUD"] = 1.50m, ["JPY"] = 150m },
                ["EUR"] = new() { ["USD"] = 1.08m, ["GBP"] = 0.86m, ["ZAR"] = 20.0m, ["CAD"] = 1.45m, ["AUD"] = 1.61m, ["JPY"] = 161m },
                ["GBP"] = new() { ["USD"] = 1.25m, ["EUR"] = 1.16m, ["ZAR"] = 23.1m, ["CAD"] = 1.69m, ["AUD"] = 1.88m, ["JPY"] = 188m },
                ["ZAR"] = new() { ["USD"] = 0.054m, ["EUR"] = 0.050m, ["GBP"] = 0.043m, ["CAD"] = 0.073m, ["AUD"] = 0.081m, ["JPY"] = 8.1m }
            };

            return fallbackRates.GetValueOrDefault(fromCurrency, new())
                               .GetValueOrDefault(toCurrency, 1.0m);
        }

        private async Task SeedDefaultCurrenciesAsync()
        {
            var currencies = new List<Currency>();
            
            foreach (var code in _options.SupportedCurrencies ?? new[] { "USD", "EUR", "GBP", "ZAR", "CAD", "AUD", "JPY" })
            {
                currencies.Add(new Currency
                {
                    Code = code,
                    Name = _currencyNames.GetValueOrDefault(code, code),
                    Symbol = _currencySymbols.GetValueOrDefault(code, code),
                    IsActive = true,
                    DecimalPlaces = code == "JPY" ? 0 : 2,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Currencies.AddRange(currencies);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Seeded {Count} default currencies", currencies.Count);
        }

        private int GetCurrencyDecimalPlaces(string currency)
        {
            return currency switch
            {
                "JPY" => 0,
                _ => 2
            };
        }
    }

    public class PricingOptions
    {
        public string DefaultCurrency { get; set; } = "USD";
        public string[]? SupportedCurrencies { get; set; }
        public TimeSpan CacheExchangeRatesFor { get; set; } = TimeSpan.FromHours(1);
    }
}