using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace PosBackend.Middlewares
{
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ResponseCachingMiddleware> _logger;

        public ResponseCachingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<ResponseCachingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only cache GET requests
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // Skip caching for certain paths
            if (ShouldSkipCaching(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Create a cache key from the request path, query string, and user context
            var cacheKey = GenerateCacheKey(context.Request);

            // Check if the response is in the cache
            if (_cache.TryGetValue(cacheKey, out CachedResponse? cachedResponse) && cachedResponse != null)
            {
                _logger.LogDebug("Cache hit for {Path}", context.Request.Path);
                
                // Set cached headers
                foreach (var header in cachedResponse.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }
                
                context.Response.ContentType = cachedResponse.ContentType;
                context.Response.StatusCode = cachedResponse.StatusCode;
                
                await context.Response.WriteAsync(cachedResponse.Body);
                return;
            }

            // Store the original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                // Create a new memory stream to capture the response
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Call the next middleware
                await _next(context);

                // If the response is successful, cache it
                if (context.Response.StatusCode == 200)
                {
                    // Reset the memory stream position
                    responseBody.Seek(0, SeekOrigin.Begin);

                    // Read the response
                    var responseText = Encoding.UTF8.GetString(responseBody.ToArray());

                    var responseToCache = new CachedResponse
                    {
                        Body = responseText,
                        ContentType = context.Response.ContentType ?? "application/json",
                        StatusCode = context.Response.StatusCode,
                        Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
                    };

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = GetCacheDuration(context.Request.Path),
                        SlidingExpiration = TimeSpan.FromMinutes(2),
                        Priority = CacheItemPriority.Normal
                    };

                    // Cache the response
                    _cache.Set(cacheKey, responseToCache, cacheOptions);
                    _logger.LogDebug("Cached response for {Path}", context.Request.Path);

                    // Reset the memory stream position
                    responseBody.Seek(0, SeekOrigin.Begin);
                }

                // Copy the response to the original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                // Restore the original response body stream
                context.Response.Body = originalBodyStream;
            }
        }

        private static bool ShouldSkipCaching(PathString path)
        {
            var pathValue = path.Value?.ToLower();
            return pathValue == null ||
                   pathValue.Contains("/swagger") ||
                   pathValue.Contains("/health") ||
                   pathValue.Contains("/auth") ||
                   pathValue.Contains("/webhook") ||
                   pathValue.Contains("/upload");
        }

        private static string GenerateCacheKey(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(request.Path);
            
            if (request.QueryString.HasValue)
            {
                keyBuilder.Append(request.QueryString.Value);
            }

            // Include user identity in cache key for user-specific data
            if (request.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                keyBuilder.Append($"_user_{request.HttpContext.User.Identity.Name}");
            }

            return keyBuilder.ToString();
        }

        private static TimeSpan GetCacheDuration(PathString path)
        {
            var pathValue = path.Value?.ToLower();
            
            return pathValue switch
            {
                var p when p?.Contains("/summary") == true => TimeSpan.FromMinutes(10), // Dropdown data
                var p when p?.Contains("/products") == true => TimeSpan.FromMinutes(5),  // Product listings
                var p when p?.Contains("/customers") == true => TimeSpan.FromMinutes(3), // Customer data changes more frequently
                var p when p?.Contains("/categories") == true => TimeSpan.FromMinutes(15), // Categories change less frequently
                _ => TimeSpan.FromMinutes(5) // Default cache duration
            };
        }
    }

    public class CachedResponse
    {
        public string Body { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
    }
}
