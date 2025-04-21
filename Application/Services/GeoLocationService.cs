using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using System;
using System.Net;
using System.IO;

namespace PosBackend.Services
{
    public class GeoLocationService
    {
        private readonly DatabaseReader? _reader;

        public GeoLocationService(string dbPath)
        {
            if (dbPath == "fallback")
            {
                return;
            }
            if (File.Exists(dbPath))
            {
                _reader = new DatabaseReader(dbPath);
            }
            else
            {
                Console.WriteLine($"GeoLite2 database file not found at: {dbPath}");
                _reader = null;
            }
        }
        public virtual string GetCountryCode(string ipAddress)
        {
            try
            {
                if (!IPAddress.TryParse(ipAddress, out IPAddress? ip))
                {
                    return "US";
                }

                if (_reader == null)
                {
                    return "US";
                }

                CountryResponse response = _reader.Country(ip);
                return response.Country?.IsoCode ?? "US";
            }
            catch (Exception)
            {
                return "US";
            }
        }
    }
}