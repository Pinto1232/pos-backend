using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;
using PosBackend.Services.Interfaces;
using System.Globalization;

namespace PosBackend.Services
{
    public class CurrencyDetectionService : ICurrencyDetectionService
    {
        private readonly PosDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly PosBackend.Services.GeoLocationService _geoService;
        private readonly ILogger<CurrencyDetectionService> _logger;
        private readonly PricingOptions _options;

        private readonly Dictionary<string, string> _countryToCurrency = new()
        {
            // Major countries
            { "US", "USD" }, { "CA", "CAD" }, { "GB", "GBP" }, { "AU", "AUD" },
            { "JP", "JPY" }, { "ZA", "ZAR" }, { "NZ", "NZD" }, { "CH", "CHF" },
            
            // Eurozone countries
            { "DE", "EUR" }, { "FR", "EUR" }, { "IT", "EUR" }, { "ES", "EUR" },
            { "NL", "EUR" }, { "BE", "EUR" }, { "AT", "EUR" }, { "PT", "EUR" },
            { "FI", "EUR" }, { "IE", "EUR" }, { "LU", "EUR" }, { "GR", "EUR" },
            { "CY", "EUR" }, { "MT", "EUR" }, { "SI", "EUR" }, { "SK", "EUR" },
            { "EE", "EUR" }, { "LV", "EUR" }, { "LT", "EUR" }, { "HR", "EUR" },
            
            // Other regions
            { "IN", "INR" }, { "CN", "CNY" }, { "BR", "BRL" }, { "MX", "MXN" },
            { "AR", "ARS" }, { "CL", "CLP" }, { "CO", "COP" }, { "PE", "PEN" },
            { "RU", "RUB" }, { "TR", "TRY" }, { "EG", "EGP" }, { "NG", "NGN" },
            { "KE", "KES" }, { "GH", "GHS" }, { "TH", "THB" }, { "VN", "VND" },
            { "ID", "IDR" }, { "MY", "MYR" }, { "SG", "SGD" }, { "PH", "PHP" },
            { "KR", "KRW" }, { "TW", "TWD" }, { "HK", "HKD" }, { "IL", "ILS" },
            { "AE", "AED" }, { "SA", "SAR" }, { "QA", "QAR" }, { "KW", "KWD" },
            { "BH", "BHD" }, { "OM", "OMR" }, { "JO", "JOD" }, { "LB", "LBP" }
        };

        private readonly Dictionary<string, string> _languageToCurrency = new()
        {
            { "en-US", "USD" }, { "en-CA", "CAD" }, { "en-GB", "GBP" }, { "en-AU", "AUD" },
            { "en-NZ", "NZD" }, { "en-ZA", "ZAR" }, { "de-DE", "EUR" }, { "fr-FR", "EUR" },
            { "it-IT", "EUR" }, { "es-ES", "EUR" }, { "nl-NL", "EUR" }, { "pt-PT", "EUR" },
            { "ja-JP", "JPY" }, { "zh-CN", "CNY" }, { "ko-KR", "KRW" }, { "th-TH", "THB" },
            { "vi-VN", "VND" }, { "id-ID", "IDR" }, { "ms-MY", "MYR" }, { "hi-IN", "INR" },
            { "ar-SA", "SAR" }, { "he-IL", "ILS" }, { "tr-TR", "TRY" }, { "ru-RU", "RUB" },
            { "pt-BR", "BRL" }, { "es-MX", "MXN" }, { "es-AR", "ARS" }, { "es-CL", "CLP" }
        };

        public CurrencyDetectionService(
            PosDbContext context,
            ICacheService cacheService,
            PosBackend.Services.GeoLocationService geoService,
            ILogger<CurrencyDetectionService> logger,
            IOptions<PricingOptions> options)
        {
            _context = context;
            _cacheService = cacheService;
            _geoService = geoService;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<string> DetectCurrencyAsync(HttpContext context)
        {
            try
            {
                // 1. Check user authentication and saved preference
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var userId = context.User.FindFirst("sub")?.Value ?? 
                                context.User.FindFirst("id")?.Value;
                    
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var userCurrency = await GetUserPreferredCurrencyAsync(userId);
                        if (!string.IsNullOrEmpty(userCurrency))
                        {
                            _logger.LogDebug("Using user preferred currency: {Currency} for user {UserId}", userCurrency, userId);
                            return userCurrency;
                        }
                    }
                }

                // 2. Check explicit currency parameter in query string
                if (context.Request.Query.ContainsKey("currency"))
                {
                    var queryCurrency = context.Request.Query["currency"].ToString().ToUpper();
                    if (IsCurrencySupported(queryCurrency))
                    {
                        _logger.LogDebug("Using currency from query parameter: {Currency}", queryCurrency);
                        return queryCurrency;
                    }
                }

                // 3. Check currency header
                if (context.Request.Headers.ContainsKey("X-Currency"))
                {
                    var headerCurrency = context.Request.Headers["X-Currency"].ToString().ToUpper();
                    if (IsCurrencySupported(headerCurrency))
                    {
                        _logger.LogDebug("Using currency from header: {Currency}", headerCurrency);
                        return headerCurrency;
                    }
                }

                // 4. Check Accept-Language header
                var languageCurrency = GetCurrencyFromLanguage(context.Request.Headers["Accept-Language"]);
                if (!string.IsNullOrEmpty(languageCurrency))
                {
                    _logger.LogDebug("Using currency from Accept-Language: {Currency}", languageCurrency);
                    return languageCurrency;
                }

                // 5. Use IP-based geolocation (fallback)
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var countryCode = _geoService.GetCountryCode(ipAddress);
                var geoCurrency = MapCountryToCurrency(countryCode);
                
                if (!string.IsNullOrEmpty(geoCurrency))
                {
                    _logger.LogDebug("Using currency from geo-location {CountryCode}: {Currency}", countryCode, geoCurrency);
                    return geoCurrency;
                }

                // 6. Default fallback
                var defaultCurrency = _options.DefaultCurrency;
                _logger.LogDebug("Using default currency: {Currency}", defaultCurrency);
                return defaultCurrency;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error detecting currency, falling back to default");
                return _options.DefaultCurrency;
            }
        }

        public async Task<string?> GetUserPreferredCurrencyAsync(string userId)
        {
            try
            {
                var cacheKey = $"user_currency_{userId}";
                var cached = await _cacheService.GetAsync<string>(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    return cached;
                }

                // Check if you have a UserPreferences table or similar
                // For now, return null as we don't have user preferences table
                // In a real implementation, you'd have something like:
                // var preference = await _context.UserPreferences
                //     .Where(p => p.UserId == userId && p.Key == "PreferredCurrency")
                //     .FirstOrDefaultAsync();
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user preferred currency for user {UserId}", userId);
                return null;
            }
        }

        public async Task SetUserPreferredCurrencyAsync(string userId, string currency)
        {
            try
            {
                if (!IsCurrencySupported(currency))
                {
                    throw new ArgumentException($"Currency {currency} is not supported");
                }

                // Cache the preference
                var cacheKey = $"user_currency_{userId}";
                await _cacheService.SetAsync(cacheKey, currency, null, TimeSpan.FromDays(30));

                // In a real implementation, you'd save to database:
                // var preference = await _context.UserPreferences
                //     .FirstOrDefaultAsync(p => p.UserId == userId && p.Key == "PreferredCurrency");
                // 
                // if (preference == null)
                // {
                //     _context.UserPreferences.Add(new UserPreference 
                //     { 
                //         UserId = userId, 
                //         Key = "PreferredCurrency", 
                //         Value = currency 
                //     });
                // }
                // else
                // {
                //     preference.Value = currency;
                // }
                // 
                // await _context.SaveChangesAsync();

                _logger.LogInformation("Set preferred currency {Currency} for user {UserId}", currency, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting preferred currency {Currency} for user {UserId}", currency, userId);
                throw;
            }
        }

        public string MapCountryToCurrency(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return _options.DefaultCurrency;

            var currency = _countryToCurrency.GetValueOrDefault(countryCode.ToUpper(), _options.DefaultCurrency);
            
            // Ensure the mapped currency is supported
            return IsCurrencySupported(currency) ? currency : _options.DefaultCurrency;
        }

        public string? GetCurrencyFromLanguage(string? acceptLanguage)
        {
            if (string.IsNullOrEmpty(acceptLanguage))
                return null;

            try
            {
                // Parse Accept-Language header format: "en-US,en;q=0.9,es;q=0.8"
                var languages = acceptLanguage.Split(',')
                    .Select(lang => lang.Split(';')[0].Trim())
                    .ToList();

                foreach (var language in languages)
                {
                    // Try exact match first
                    if (_languageToCurrency.TryGetValue(language, out var exactCurrency))
                    {
                        if (IsCurrencySupported(exactCurrency))
                            return exactCurrency;
                    }

                    // Try language part only (e.g., "en" from "en-US")
                    if (language.Contains('-'))
                    {
                        var languageCode = language.Split('-')[0];
                        var fallbackCurrency = _languageToCurrency
                            .Where(kvp => kvp.Key.StartsWith($"{languageCode}-"))
                            .Select(kvp => kvp.Value)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(fallbackCurrency) && IsCurrencySupported(fallbackCurrency))
                            return fallbackCurrency;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing Accept-Language header: {AcceptLanguage}", acceptLanguage);
                return null;
            }
        }

        private bool IsCurrencySupported(string currency)
        {
            var supportedCurrencies = _options.SupportedCurrencies ?? 
                new[] { "USD", "EUR", "GBP", "ZAR", "CAD", "AUD", "JPY" };
            
            return supportedCurrencies.Contains(currency.ToUpper());
        }
    }
}