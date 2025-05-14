using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PosBackend.Application.Services.Caching
{
    /// <summary>
    /// Implementation of ICacheService using IMemoryCache
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheConfiguration _configuration;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, string> _keys = new ConcurrentDictionary<string, string>();

        public MemoryCacheService(
            IMemoryCache cache,
            CacheConfiguration configuration,
            ILogger<MemoryCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public T? Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var result = _cache.TryGetValue(key, out T? value);
            
            if (_configuration.EnableLogging)
            {
                if (result)
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                else
                    _logger.LogDebug("Cache miss for key: {Key}", key);
            }

            return value;
        }

        /// <inheritdoc />
        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        /// <inheritdoc />
        public T GetOrSet<T>(string key, Func<T> factory)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_cache.TryGetValue(key, out T? value))
            {
                if (_configuration.EnableLogging)
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                return value!;
            }

            if (_configuration.EnableLogging)
                _logger.LogDebug("Cache miss for key: {Key}, fetching data", key);

            value = factory();

            var options = new MemoryCacheEntryOptions();
            
            if (_configuration.UseDefaultSlidingExpiration)
            {
                options.SlidingExpiration = _configuration.GetSlidingExpiration(key);
            }
            else
            {
                options.AbsoluteExpiration = _configuration.GetAbsoluteExpiration(key);
            }

            // Register the key for tracking
            _keys.TryAdd(key, key);
            options.RegisterPostEvictionCallback(OnPostEviction);

            _cache.Set(key, value, options);
            return value;
        }

        /// <inheritdoc />
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_cache.TryGetValue(key, out T? value))
            {
                if (_configuration.EnableLogging)
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                return value!;
            }

            if (_configuration.EnableLogging)
                _logger.LogDebug("Cache miss for key: {Key}, fetching data", key);

            value = await factory();

            var options = new MemoryCacheEntryOptions();
            
            if (_configuration.UseDefaultSlidingExpiration)
            {
                options.SlidingExpiration = _configuration.GetSlidingExpiration(key);
            }
            else
            {
                options.AbsoluteExpiration = _configuration.GetAbsoluteExpiration(key);
            }

            // Register the key for tracking
            _keys.TryAdd(key, key);
            options.RegisterPostEvictionCallback(OnPostEviction);

            _cache.Set(key, value, options);
            return value;
        }

        /// <inheritdoc />
        public void Set<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var options = new MemoryCacheEntryOptions();
            
            if (slidingExpiration.HasValue)
            {
                options.SlidingExpiration = slidingExpiration.Value;
            }
            else if (absoluteExpiration.HasValue)
            {
                options.AbsoluteExpiration = absoluteExpiration.Value;
            }
            else if (_configuration.UseDefaultSlidingExpiration)
            {
                options.SlidingExpiration = _configuration.GetSlidingExpiration(key);
            }
            else
            {
                options.AbsoluteExpiration = _configuration.GetAbsoluteExpiration(key);
            }

            // Register the key for tracking
            _keys.TryAdd(key, key);
            options.RegisterPostEvictionCallback(OnPostEviction);

            _cache.Set(key, value, options);
            
            if (_configuration.EnableLogging)
                _logger.LogDebug("Set cache for key: {Key}", key);
        }

        /// <inheritdoc />
        public Task SetAsync<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            Set(key, value, absoluteExpiration, slidingExpiration);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            _cache.Remove(key);
            _keys.TryRemove(key, out _);
            
            if (_configuration.EnableLogging)
                _logger.LogDebug("Removed cache for key: {Key}", key);
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RemoveByPrefix(string keyPrefix)
        {
            if (string.IsNullOrEmpty(keyPrefix))
                throw new ArgumentNullException(nameof(keyPrefix));

            var keysToRemove = _keys.Keys
                .Where(k => k.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
            
            if (_configuration.EnableLogging)
                _logger.LogDebug("Removed {Count} cache entries with prefix: {Prefix}", keysToRemove.Count, keyPrefix);
        }

        /// <inheritdoc />
        public Task RemoveByPrefixAsync(string keyPrefix)
        {
            RemoveByPrefix(keyPrefix);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Clear()
        {
            var keysToRemove = _keys.Keys.ToList();
            foreach (var key in keysToRemove)
            {
                Remove(key);
            }
            
            if (_configuration.EnableLogging)
                _logger.LogDebug("Cleared entire cache ({Count} entries)", keysToRemove.Count);
        }

        /// <inheritdoc />
        public Task ClearAsync()
        {
            Clear();
            return Task.CompletedTask;
        }

        private void OnPostEviction(object key, object? value, EvictionReason reason, object? state)
        {
            var stringKey = key.ToString();
            if (stringKey != null)
            {
                _keys.TryRemove(stringKey, out _);
                
                if (_configuration.EnableLogging)
                    _logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", stringKey, reason);
            }
        }
    }
}
