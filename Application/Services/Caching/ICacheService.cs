using System;
using System.Threading.Tasks;

namespace PosBackend.Application.Services.Caching
{
    /// <summary>
    /// Interface for the cache service
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets an item from the cache
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns>The cached item, or default(T) if not found</returns>
        T? Get<T>(string key);

        /// <summary>
        /// Gets an item from the cache asynchronously
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns>The cached item, or default(T) if not found</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Gets an item from the cache, or sets it if not found
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="factory">A function that returns the item to cache</param>
        /// <returns>The cached item, or the result of the factory function</returns>
        T GetOrSet<T>(string key, Func<T> factory);

        /// <summary>
        /// Gets an item from the cache, or sets it if not found asynchronously
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="factory">A function that returns the item to cache</param>
        /// <returns>The cached item, or the result of the factory function</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory);

        /// <summary>
        /// Sets an item in the cache
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The item to cache</param>
        /// <param name="absoluteExpiration">Optional absolute expiration time</param>
        /// <param name="slidingExpiration">Optional sliding expiration time</param>
        void Set<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

        /// <summary>
        /// Sets an item in the cache asynchronously
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The item to cache</param>
        /// <param name="absoluteExpiration">Optional absolute expiration time</param>
        /// <param name="slidingExpiration">Optional sliding expiration time</param>
        Task SetAsync<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null);

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">The cache key</param>
        void Remove(string key);

        /// <summary>
        /// Removes an item from the cache asynchronously
        /// </summary>
        /// <param name="key">The cache key</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes all items with the specified prefix from the cache
        /// </summary>
        /// <param name="keyPrefix">The cache key prefix</param>
        void RemoveByPrefix(string keyPrefix);

        /// <summary>
        /// Removes all items with the specified prefix from the cache asynchronously
        /// </summary>
        /// <param name="keyPrefix">The cache key prefix</param>
        Task RemoveByPrefixAsync(string keyPrefix);

        /// <summary>
        /// Clears the entire cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears the entire cache asynchronously
        /// </summary>
        Task ClearAsync();
    }
}
