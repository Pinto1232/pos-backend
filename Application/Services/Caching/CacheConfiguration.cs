using System;

namespace PosBackend.Application.Services.Caching
{
    public class CacheConfiguration
    {
        public int DefaultAbsoluteExpirationSeconds { get; set; } = 300;
        public int DefaultSlidingExpirationSeconds { get; set; } = 60;
        public bool UseDefaultSlidingExpiration { get; set; } = true;
        public bool EnableLogging { get; set; } = true;
        public int MaxCacheSize { get; set; } = 1000;
        public Dictionary<string, int> CustomExpirationTimes { get; set; } = new Dictionary<string, int>
        {
            { "User:", 600 },
            { "Package:", 1800 },
            { "GeoLocation:", 86400 },
            { "Product:", 300 },
            { "Category:", 600 },
            { "Inventory:", 120 },
            { "Currency:", 3600 }
        };

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

        public DateTimeOffset GetAbsoluteExpiration(string cacheKey)
        {
            return DateTimeOffset.UtcNow.AddSeconds(GetExpirationTime(cacheKey));
        }

        public TimeSpan GetSlidingExpiration(string cacheKey)
        {
            return TimeSpan.FromSeconds(UseDefaultSlidingExpiration ? 
                DefaultSlidingExpirationSeconds : 
                GetExpirationTime(cacheKey) / 2);
        }
    }
}
