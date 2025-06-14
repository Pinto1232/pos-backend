namespace PosBackend.Services.Interfaces
{
    public interface ICurrencyDetectionService
    {
        /// <summary>
        /// Detect user's preferred currency based on various factors
        /// </summary>
        Task<string> DetectCurrencyAsync(HttpContext context);
        
        /// <summary>
        /// Get user's saved currency preference
        /// </summary>
        Task<string?> GetUserPreferredCurrencyAsync(string userId);
        
        /// <summary>
        /// Save user's currency preference
        /// </summary>
        Task SetUserPreferredCurrencyAsync(string userId, string currency);
        
        /// <summary>
        /// Map country code to currency
        /// </summary>
        string MapCountryToCurrency(string countryCode);
        
        /// <summary>
        /// Extract currency from Accept-Language header
        /// </summary>
        string? GetCurrencyFromLanguage(string? acceptLanguage);
    }
}