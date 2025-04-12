using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using System;
using System.Net;

namespace PosBackend.Services
{
    public class GeoLocationService
    {
        private readonly DatabaseReader? _reader;

        public GeoLocationService(string dbPath)
        {
            if (dbPath == "fallback")
            {
                // In fallback scenario, do not initialize the DatabaseReader.
                return;
            }

            _reader = new DatabaseReader(dbPath);
        }

        // Marked virtual so it can be overridden.
        public virtual string GetCountryCode(string ipAddress)
        {
            try
            {
                if (!IPAddress.TryParse(ipAddress, out IPAddress? ip))
                {
                    // If parsing fails, default to "US"
                    return "US";
                }

                if (_reader == null)
                {
                    // If the reader is not initialized, default to "US"
                    return "US";
                }

                CountryResponse response = _reader.Country(ip);
                return response.Country?.IsoCode ?? "US";
            }
            catch (Exception)
            {
                // Log error as needed and return a default value.
                // For example: _logger.LogError("Geo lookup failed for IP {ipAddress}");
                return "US";
            }
        }
    }
}
