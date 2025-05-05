using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PosBackend.Middlewares
{
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultCacheDuration;

        public ResponseCachingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
            _defaultCacheDuration = TimeSpan.FromMinutes(5);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only cache GET requests
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // Don't cache authenticated requests by default
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            // Create a cache key from the request path and query string
            var cacheKey = $"{context.Request.Path}{context.Request.QueryString}";

            // Check if the response is in the cache
            if (_cache.TryGetValue(cacheKey, out byte[]? cachedResponse) && cachedResponse != null)
            {
                // Return cached response
                context.Response.ContentType = "application/json";
                await context.Response.Body.WriteAsync(cachedResponse, 0, cachedResponse.Length);
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
                if (context.Response.StatusCode == 200 &&
                    context.Response.ContentType != null &&
                    context.Response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    // Reset the memory stream position
                    responseBody.Seek(0, SeekOrigin.Begin);

                    // Read the response into a byte array
                    var responseBytes = responseBody.ToArray();

                    // Cache the response
                    _cache.Set(cacheKey, responseBytes, _defaultCacheDuration);

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
    }
}
