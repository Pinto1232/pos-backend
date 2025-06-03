using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PosBackend.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
        private const int MaxRequests = 100;  // Maximum requests per minute
        private const int RefillRate = 100;   // Tokens added per minute
        private const int BucketCapacity = 100;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"{clientIp}:{context.Request.Path}";

            var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(BucketCapacity, RefillRate));

            if (!bucket.TryConsume(1))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("Retry-After", "60");
                await context.Response.WriteAsJsonAsync(new { message = "Too many requests. Please try again later." });
                return;
            }

            await _next(context);
        }

        private class TokenBucket
        {
            private readonly double _refillRate;
            private readonly int _capacity;
            private double _tokens;
            private DateTime _lastRefill;
            private readonly object _lock = new();

            public TokenBucket(int capacity, double refillRate)
            {
                _capacity = capacity;
                _refillRate = refillRate;
                _tokens = capacity;
                _lastRefill = DateTime.UtcNow;
            }

            public bool TryConsume(int tokens)
            {
                lock (_lock)
                {
                    RefillTokens();

                    if (_tokens < tokens)
                        return false;

                    _tokens -= tokens;
                    // Ensure tokens never go negative
                    if (_tokens < 0)
                        _tokens = 0;
                    return true;
                }
            }

            private void RefillTokens()
            {
                var now = DateTime.UtcNow;
                var timePassed = (now - _lastRefill).TotalMinutes;
                var tokensToAdd = timePassed * _refillRate;

                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefill = now;
            }
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
