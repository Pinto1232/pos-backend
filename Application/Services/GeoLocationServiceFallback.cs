using System;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Services.Caching;

namespace PosBackend.Services
{
    public class GeoLocationServiceFallback : GeoLocationService
    {
        public GeoLocationServiceFallback() : base("fallback")
        { }

        public GeoLocationServiceFallback(ICacheService cacheService, ILogger<GeoLocationService> logger)
            : base("fallback", cacheService, logger)
        { }

        public override string GetCountryCode(string ipAddress)
        {
            return "US";
        }
    }
}