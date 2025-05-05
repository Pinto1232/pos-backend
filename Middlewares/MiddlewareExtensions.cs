using Microsoft.AspNetCore.Builder;

namespace PosBackend.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomResponseCaching(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseCachingMiddleware>();
        }
    }
}
