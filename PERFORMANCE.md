# Backend Performance Optimizations

This document outlines the performance optimizations implemented in the backend of the Pisval Tech POS system.

## Caching Architecture

We've implemented a comprehensive caching strategy with multiple layers:

1. **Built-in ASP.NET Core Response Caching**: Provides standard HTTP caching based on cache headers.
2. **Custom Response Caching Middleware**: Provides more fine-grained control over what gets cached and for how long.
3. **In-Memory Service-Level Caching**: Caches expensive database queries and business logic operations.

### Response Caching

- Only GET requests are cached
- Authenticated requests are not cached by default
- Responses are cached for 5 minutes by default
- Only successful (200 OK) JSON responses are cached

### In-Memory Service Caching

We use a custom `ICacheService` wrapper around .NET's `IMemoryCache` to provide:

- Configurable cache expiration policies (absolute and sliding)
- Different cache durations for different types of data
- Cache invalidation when data changes
- Cache hit/miss logging for monitoring
- Prefix-based cache key management

#### Key Features

- **Configurable Expiration**: Different cache durations for different data types
- **Cache Invalidation**: Automatic cache invalidation when data is modified
- **Monitoring**: Cache hit/miss logging and statistics endpoint
- **Thread Safety**: Thread-safe implementation for concurrent access

## Database Optimizations

### Connection Resiliency

- Automatic retry on connection failures (up to 5 times)
- Configurable command timeout (30 seconds)
- Query tracking disabled for read-only operations to improve performance

### Database Indexes

We've added indexes to frequently queried columns to improve query performance:

- Primary tables (Products, Customers, Sales, Inventory, Orders)
- Composite indexes for frequently joined tables
- Full-text search indexes
- Timestamp column indexes for filtering

## Response Compression

Response compression is enabled for all API responses to reduce bandwidth usage and improve load times:

- Enabled for HTTPS connections
- Configured for various MIME types (JSON, HTML, CSS, JavaScript, etc.)

## Cache Control Headers

Static assets are configured with appropriate cache control headers:

- JavaScript and CSS files are cached for 1 year (31536000 seconds)
- Static files in the /static directory are cached for 1 year

## How to Run Database Optimizations

To apply the database optimizations, run the following command from the backend directory:

```bash
cd Scripts
./optimize-database.sh
```

This script will:
1. Create all the necessary indexes
2. Run VACUUM ANALYZE to update statistics

## Monitoring Performance

To monitor the performance of your API:

1. Check the logs for slow query warnings
2. Use the Swagger UI to test API response times
3. Monitor database performance using PostgreSQL's built-in tools

## Monitoring Cache Performance

To monitor the cache performance:

1. Use the `/api/Cache/stats` endpoint to view cache statistics (Admin role required)
2. Check the logs for cache hit/miss information
3. Run the `Scripts/test-cache-performance.ps1` script to measure cache performance

## Testing Cache Performance

We've included a PowerShell script to test the cache performance:

```powershell
cd Scripts
./test-cache-performance.ps1
```

This script will:
1. Test API response times with a cold cache
2. Test API response times with a warm cache
3. Calculate the performance improvement

## Cache Management

For administrators, we provide cache management endpoints:

- `POST /api/Cache/clear` - Clears the entire cache
- `POST /api/Cache/clear/{key}` - Clears a specific cache key
- `POST /api/Cache/clear/prefix/{prefix}` - Clears all cache keys with a specific prefix
- `GET /api/Cache/keys` - Gets all cache keys (for debugging)
- `GET /api/Cache/stats` - Gets cache statistics

## Future Improvements

Consider implementing these additional optimizations:

1. **Distributed Caching**: Add Redis for distributed caching in multi-server environments
2. **Cache Partitioning**: Implement cache partitioning for better memory management
3. **Cache Warming**: Implement cache warming for frequently accessed data
4. **API Rate Limiting**: Implement rate limiting to prevent abuse
5. **Query Optimization Monitoring**: Add database query optimization monitoring
6. **Asynchronous Processing**: Use asynchronous processing for long-running tasks
