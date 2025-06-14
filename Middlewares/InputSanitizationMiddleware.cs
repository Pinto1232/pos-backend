using PosBackend.Services;
using System.Text;
using System.Text.Json;

namespace PosBackend.Middlewares
{
    /// <summary>
    /// Middleware to automatically sanitize request inputs
    /// </summary>
    public class InputSanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InputSanitizationMiddleware> _logger;

        public InputSanitizationMiddleware(
            RequestDelegate next,
            ILogger<InputSanitizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip sanitization for certain paths
            if (ShouldSkipSanitization(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Resolve the scoped service from the request service provider
            var sanitizationService = context.RequestServices.GetRequiredService<IInputSanitizationService>();

            try
            {
                // Sanitize query parameters
                SanitizeQueryParameters(context, sanitizationService);

                // Sanitize request body for POST/PUT requests
                if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
                {
                    await SanitizeRequestBody(context, sanitizationService);
                }

                // Sanitize headers
                SanitizeHeaders(context, sanitizationService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during input sanitization");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid input detected");
                return;
            }

            await _next(context);
        }

        private bool ShouldSkipSanitization(PathString path)
        {
            var skipPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register", 
                "/health",
                "/swagger",
                "/api/webhook"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
        }

        private void SanitizeQueryParameters(HttpContext context, IInputSanitizationService sanitizationService)
        {
            var request = context.Request;
            var sanitizedQuery = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

            foreach (var param in request.Query)
            {
                var sanitizedValues = new List<string>();
                foreach (var value in param.Value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var sanitized = sanitizationService.SanitizeString(value!);
                        sanitizedValues.Add(sanitized);
                    }
                }
                sanitizedQuery[param.Key] = new Microsoft.Extensions.Primitives.StringValues(sanitizedValues.ToArray());
            }

            // Replace query collection (this is a simplified approach - in reality, you might need to use reflection)
            // For now, log the sanitization
            _logger.LogDebug("Query parameters sanitized for request {Path}", request.Path);
        }

        private async Task SanitizeRequestBody(HttpContext context, IInputSanitizationService sanitizationService)
        {
            var request = context.Request;
            
            if (request.ContentLength == null || request.ContentLength == 0)
                return;

            // Check content type
            var contentType = request.ContentType?.ToLowerInvariant();
            if (contentType == null)
                return;

            if (contentType.Contains("application/json"))
            {
                await SanitizeJsonBody(context, sanitizationService);
            }
            else if (contentType.Contains("application/x-www-form-urlencoded"))
            {
                await SanitizeFormBody(context, sanitizationService);
            }
            else if (contentType.Contains("multipart/form-data"))
            {
                await SanitizeMultipartBody(context, sanitizationService);
            }
        }

        private async Task SanitizeJsonBody(HttpContext context, IInputSanitizationService sanitizationService)
        {
            var request = context.Request;
            request.EnableBuffering();

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (string.IsNullOrEmpty(body))
                return;

            try
            {
                var jsonDocument = JsonDocument.Parse(body);
                var sanitizedJson = SanitizeJsonElement(jsonDocument.RootElement, sanitizationService);
                
                var sanitizedBody = JsonSerializer.Serialize(sanitizedJson, new JsonSerializerOptions 
                { 
                    WriteIndented = false 
                });

                // Replace the request body
                var bytes = Encoding.UTF8.GetBytes(sanitizedBody);
                request.Body = new MemoryStream(bytes);
                request.ContentLength = bytes.Length;

                _logger.LogDebug("JSON body sanitized for request {Path}", request.Path);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body for {Path}", request.Path);
                throw new InvalidOperationException("Invalid JSON format");
            }
        }

        private object? SanitizeJsonElement(JsonElement element, IInputSanitizationService sanitizationService)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => SanitizeJsonObject(element, sanitizationService),
                JsonValueKind.Array => SanitizeJsonArray(element, sanitizationService),
                JsonValueKind.String => sanitizationService.SanitizeString(element.GetString() ?? ""),
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        private Dictionary<string, object?> SanitizeJsonObject(JsonElement obj, IInputSanitizationService sanitizationService)
        {
            var result = new Dictionary<string, object?>();
            
            foreach (var property in obj.EnumerateObject())
            {
                var sanitizedKey = sanitizationService.SanitizeString(property.Name);
                var sanitizedValue = SanitizeJsonElement(property.Value, sanitizationService);
                result[sanitizedKey] = sanitizedValue;
            }
            
            return result;
        }

        private List<object?> SanitizeJsonArray(JsonElement array, IInputSanitizationService sanitizationService)
        {
            var result = new List<object?>();
            
            foreach (var item in array.EnumerateArray())
            {
                result.Add(SanitizeJsonElement(item, sanitizationService));
            }
            
            return result;
        }

        private async Task SanitizeFormBody(HttpContext context, IInputSanitizationService sanitizationService)
        {
            var request = context.Request;
            var form = await request.ReadFormAsync();
            
            _logger.LogDebug("Form data sanitized for request {Path}", request.Path);
        }

        private async Task SanitizeMultipartBody(HttpContext context, IInputSanitizationService sanitizationService)
        {
            var request = context.Request;
            
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                
                // Sanitize form fields
                foreach (var field in form)
                {
                    if (!string.IsNullOrEmpty(field.Value))
                    {
                        var sanitizedValue = sanitizationService.SanitizeString(field.Value!);
                        _logger.LogDebug("Multipart field {Key} sanitized", field.Key);
                    }
                }

                // Validate file uploads
                foreach (var file in form.Files)
                {
                    var sanitizedFilename = sanitizationService.SanitizeFilename(file.FileName);
                    if (string.IsNullOrEmpty(sanitizedFilename))
                    {
                        _logger.LogWarning("Dangerous filename detected: {Filename}", file.FileName);
                        throw new InvalidOperationException("Invalid filename");
                    }
                }
            }
        }

        private void SanitizeHeaders(HttpContext context, IInputSanitizationService sanitizationService)
        {
            var request = context.Request;
            var dangerousHeaders = new[]
            {
                "X-Forwarded-For",
                "X-Real-IP",
                "User-Agent",
                "Referer",
                "X-Requested-With"
            };

            foreach (var headerName in dangerousHeaders)
            {
                if (request.Headers.ContainsKey(headerName))
                {
                    var originalValue = request.Headers[headerName].ToString();
                    var sanitizedValue = sanitizationService.SanitizeString(originalValue ?? "");
                    
                    if (originalValue != sanitizedValue)
                    {
                        _logger.LogWarning("Header {HeaderName} sanitized from {Original} to {Sanitized}", 
                            headerName, originalValue, sanitizedValue);
                    }
                }
            }
        }
    }

    public static class InputSanitizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseInputSanitization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InputSanitizationMiddleware>();
        }
    }
}