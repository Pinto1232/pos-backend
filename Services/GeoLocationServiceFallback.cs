namespace PosBackend.Services
{
    // Inherits from GeoLocationService.
    public class GeoLocationServiceFallback : GeoLocationService
    {
        public GeoLocationServiceFallback() : base("fallback")
        {
        }

        // Override the base implementation to always return "US"
        public override string GetCountryCode(string ipAddress)
        {
            return "US";
        }
    }
}
