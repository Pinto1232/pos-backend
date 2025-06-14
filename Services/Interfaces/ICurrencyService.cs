using PosBackend.Models;

namespace PosBackend.Services.Interfaces
{
    public interface ICurrencyService
    {
        /// <summary>
        /// Convert amount from one currency to another
        /// </summary>
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
        
        /// <summary>
        /// Get exchange rates for a base currency
        /// </summary>
        Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string baseCurrency);
        
        /// <summary>
        /// Get current exchange rate between two currencies
        /// </summary>
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        
        /// <summary>
        /// Get supported currencies
        /// </summary>
        Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync();
        
        /// <summary>
        /// Refresh exchange rates from external API
        /// </summary>
        Task RefreshExchangeRatesAsync();
        
        /// <summary>
        /// Validate if currency is supported
        /// </summary>
        bool IsCurrencySupported(string currency);
        
        /// <summary>
        /// Get conversion result with metadata
        /// </summary>
        Task<CurrencyConversionResult> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}