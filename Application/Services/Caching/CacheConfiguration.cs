using System;

namespace PosBackend.Application.Services.Caching
{
    /// <summary>
    /// Configuration options for the cache service
    /// </summary>
    public class CacheConfiguration
    {
        /// <summary>
        /// Default absolute expiration time in seconds
        /// </summary>
        public int DefaultAbsoluteExpirationSeconds { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Default sliding expiration time in seconds
        /// </summary>
        public int DefaultSlidingExpirationSeconds { get; set; } = 60; // 1 minute

        /// <summary>
        /// Whether to use sliding expiration by default
        /// </summary>
        public bool UseDefaultSlidingExpiration { get; set; } = true;

        /// <summary>
        /// Whether to enable cache hit/miss logging
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Maximum size of the cache in entries (0 = unlimited)
        /// </summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>
        /// Cache expiration times for specific cache keys or prefixes
        /// </summary>
        public Dictionary<string, int> CustomExpirationTimes { get; set; } = new Dictionary<string, int>
        {
            // Examples:
            { "User:", 600 },           // User data: 10 minutes
            { "Package:", 1800 },        // Package data: 30 minutes
            { "GeoLocation:", 86400 },   // GeoLocation data: 24 hours
            { "Product:", 300 },         // Product data: 5 minutes
            { "Category:", 600 },        // Category data: 10 minutes
            { "Inventory:", 120 },       // Inventory data: 2 minutes (more frequent updates)
            { "Currency:", 3600 }        // Currency data: 1 hour
        };

        /// <summary>
        /// Gets the expiration time for a specific cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <returns>The expiration time in seconds</returns>
        public int GetExpirationTime(string cacheKey)
        {
            foreach (var prefix in CustomExpirationTimes.Keys)
            {
                if (cacheKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return CustomExpirationTimes[prefix];
                }
            }

            return DefaultAbsoluteExpirationSeconds;
        }

        /// <summary>
        /// Gets the absolute expiration time for a specific cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <returns>The absolute expiration time</returns>
        public DateTimeOffset GetAbsoluteExpiration(string cacheKey)
        {
            return DateTimeOffset.UtcNow.AddSeconds(GetExpirationTime(cacheKey));
        }

        /// <summary>
        /// Gets the sliding expiration time for a specific cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <returns>The sliding expiration time</returns>
        public TimeSpan GetSlidingExpiration(string cacheKey)
        {
            return TimeSpan.FromSeconds(UseDefaultSlidingExpiration ? 
                DefaultSlidingExpirationSeconds : 
                GetExpirationTime(cacheKey) / 2);
        }
    }
}
