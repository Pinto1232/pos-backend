using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Services.Caching;
using System;
using System.Net;
using System.IO;
using AppCacheKeys = PosBackend.Application.Services.Caching.CacheKeys;

namespace PosBackend.Services
{
    public class GeoLocationService
    {
        private readonly DatabaseReader? _reader;
        private readonly ICacheService? _cacheService;
        private readonly ILogger<GeoLocationService>? _logger;

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

        public GeoLocationService(string dbPath, ICacheService cacheService, ILogger<GeoLocationService> logger)
            : this(dbPath)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual string GetCountryCode(string ipAddress)
        {
            try
            {
                // If caching is available, try to get from cache first
                if (_cacheService != null)
                {
                    string cacheKey = AppCacheKeys.GeoLocation(ipAddress);
                    return _cacheService.GetOrSet(cacheKey, () => LookupCountryCode(ipAddress));
                }

                // If no cache service, perform direct lookup
                return LookupCountryCode(ipAddress);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting country code for IP: {IpAddress}", ipAddress);
                return "US";
            }
        }

        private string LookupCountryCode(string ipAddress)
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
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error looking up country code for IP: {IpAddress}", ipAddress);
                return "US";
            }
        }
    }
}