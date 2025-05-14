using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PosBackend.Application.Services.Caching;
using System.Reflection;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheController> _logger;

        public CacheController(
            ICacheService cacheService,
            IMemoryCache memoryCache,
            ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// Clears the entire cache
        /// </summary>
        /// <returns>A message indicating the cache was cleared</returns>
        [HttpPost("clear")]
        public async Task<IActionResult> ClearCache()
        {
            await _cacheService.ClearAsync();
            return Ok(new { message = "Cache cleared successfully" });
        }

        /// <summary>
        /// Clears a specific cache key
        /// </summary>
        /// <param name="key">The cache key to clear</param>
        /// <returns>A message indicating the cache key was cleared</returns>
        [HttpPost("clear/{key}")]
        public async Task<IActionResult> ClearCacheKey(string key)
        {
            await _cacheService.RemoveAsync(key);
            return Ok(new { message = $"Cache key '{key}' cleared successfully" });
        }

        /// <summary>
        /// Clears all cache keys with a specific prefix
        /// </summary>
        /// <param name="prefix">The cache key prefix to clear</param>
        /// <returns>A message indicating the cache keys were cleared</returns>
        [HttpPost("clear/prefix/{prefix}")]
        public async Task<IActionResult> ClearCachePrefix(string prefix)
        {
            await _cacheService.RemoveByPrefixAsync(prefix);
            return Ok(new { message = $"Cache keys with prefix '{prefix}' cleared successfully" });
        }

        /// <summary>
        /// Gets statistics about the cache
        /// </summary>
        /// <returns>Cache statistics</returns>
        [HttpGet("stats")]
        public IActionResult GetCacheStats()
        {
            try
            {
                // Use reflection to get the entries count from the memory cache
                var entriesCollection = GetEntriesCollection();
                
                int count = entriesCollection?.Count ?? 0;
                
                return Ok(new
                {
                    entriesCount = count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return StatusCode(500, new { message = "Error getting cache statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all cache keys (for debugging purposes only)
        /// </summary>
        /// <returns>All cache keys</returns>
        [HttpGet("keys")]
        public IActionResult GetCacheKeys()
        {
            try
            {
                var entriesCollection = GetEntriesCollection();
                
                if (entriesCollection == null)
                {
                    return Ok(new { keys = new List<string>() });
                }
                
                var keys = entriesCollection.Select(e => e.Key.ToString() ?? "").ToList();
                
                return Ok(new { keys });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache keys");
                return StatusCode(500, new { message = "Error getting cache keys", error = ex.Message });
            }
        }

        private ICollection<KeyValuePair<object, object>>? GetEntriesCollection()
        {
            // Use reflection to get the entries field from the memory cache
            var entriesField = typeof(MemoryCache).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (entriesField == null)
            {
                _logger.LogWarning("Could not find _entries field in MemoryCache");
                return null;
            }
            
            return entriesField.GetValue(_memoryCache) as ICollection<KeyValuePair<object, object>>;
        }
    }
}
