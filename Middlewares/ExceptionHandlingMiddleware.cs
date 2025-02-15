using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PosBackend.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Unhandled Exception: {ex.Message}");

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    context.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        error = "Internal Server Error",
                        message = ex.Message
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
                else
                {
                    _logger.LogWarning("⚠️ Response has already started. Exception cannot be handled gracefully.");
                }
            }
        }
    }
}