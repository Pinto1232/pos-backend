# Serilog Implementation Guide

## Overview
Serilog has been successfully implemented in the POS Backend application to provide structured logging with enhanced features.

## Configuration
Serilog is configured in `Program.cs` and customized through `appsettings.json` and `appsettings.Development.json`.

### Features Enabled:
- **Console Logging**: Formatted output to console with timestamps
- **File Logging**: Daily rolling files in the `logs/` directory
- **Structured Logging**: JSON-formatted properties for better searching
- **Log Enrichment**: Includes machine name, thread ID, and application context
- **Log Level Filtering**: Reduced noise from Microsoft frameworks

### Log File Management:
- **Production**: Retains 7 days of logs, 10MB file size limit
- **Development**: Retains 30 days of logs, 50MB file size limit
- **Rolling**: Creates new log files daily

## Usage Examples

### Basic Logging in Controllers
```csharp
public class YourController : ControllerBase
{
    private readonly ILogger<YourController> _logger;

    public YourController(ILogger<YourController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("API endpoint called at {Timestamp}", DateTime.UtcNow);
        
        try
        {
            // Your logic here
            _logger.LogDebug("Processing completed successfully");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            return StatusCode(500);
        }
    }
}
```

### Structured Logging Examples
```csharp
// Log with structured data
_logger.LogInformation("User {UserId} performed action {Action} at {Timestamp}", 
    userId, "Login", DateTime.UtcNow);

// Log with multiple properties
_logger.LogInformation("Order processed: {@Order}", new { 
    OrderId = order.Id, 
    Amount = order.Total, 
    CustomerId = order.CustomerId 
});

// Log performance metrics
using var timer = _logger.BeginScope("Operation {OperationName}", "ProcessPayment");
_logger.LogInformation("Payment processing started for order {OrderId}", orderId);
// ... processing logic
_logger.LogInformation("Payment processing completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```

### Log Levels
- **Trace**: Very detailed logs (disabled in production)
- **Debug**: Debug information (enabled in development only)
- **Information**: General application flow
- **Warning**: Unexpected behavior that doesn't stop the application
- **Error**: Error messages when something fails
- **Fatal**: Critical errors that may cause the application to terminate

### Log Scopes
Use log scopes to group related log entries:

```csharp
using (_logger.BeginScope("ProcessingOrder {OrderId}", orderId))
{
    _logger.LogInformation("Starting order validation");
    _logger.LogInformation("Order validation completed");
    _logger.LogInformation("Processing payment");
}
```

## Configuration Details

### Console Output Format
```
[14:30:22 INF] User user123 performed action Login at 2024-01-15T14:30:22Z {"UserId": "user123", "Action": "Login"}
```

### File Output Format
```
[2024-01-15 14:30:22.123 +00:00 INF] User user123 performed action Login at 2024-01-15T14:30:22Z {"UserId": "user123", "Action": "Login"}
```

## Log Files Location
- **Production**: `logs/log-YYYYMMDD.txt`
- **Development**: `logs/dev-log-YYYYMMDD.txt`

## Monitoring and Maintenance
- Log files are automatically rotated daily
- Old log files are automatically cleaned up based on retention policy
- Monitor disk space as logs can accumulate over time
- Consider implementing log aggregation for production environments

## Performance Considerations
- Structured logging has minimal performance impact
- File I/O is asynchronous to avoid blocking the main thread
- Log levels are filtered early to avoid unnecessary processing
- Use `{@property}` for complex objects to enable structured serialization

## Integration with Existing Code
The existing `ILogger<T>` dependency injection pattern continues to work seamlessly. All existing logging calls will automatically use Serilog infrastructure.

## Troubleshooting
- Check the `logs/` directory is writable
- Verify log files are being created with the correct timestamp format
- Use log level configuration to adjust verbosity
- Check console output during development for immediate feedback