using System;

namespace PosBackend.Services
{
    public class GeoLocationServiceFallback : GeoLocationService
    {
        public GeoLocationServiceFallback() : base("fallback")
        { }
        public override string GetCountryCode(string ipAddress)
        {
            return "US";
        }
    }
}