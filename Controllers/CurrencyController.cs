using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using System.IO;
using System.Net.Http.Headers;

namespace PosBackend.Controllers
{
    public class CountryCurrencyMapping
    {
        public required string CountryCode { get; set; }
        public required string CurrencyCode { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ILogger<CurrencyController> logger)
        {
            _logger = logger;
        }

        [HttpGet("location")]
        public IActionResult GetUserLocation()
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                if (ipAddress == null)
                {
                    _logger.LogWarning("Remote IP Address is null.");
                    return BadRequest("Could not determine your IP address.");
                }
                _logger.LogInformation($"Detected IP Address: {ipAddress}");

                var mappings = LoadCountryCurrencyMappings();

                var detectedCountry = DetectCountryFromIP(ipAddress);
                var currency = mappings
                    .FirstOrDefault(m => m.CountryCode == detectedCountry)
                    ?.CurrencyCode ?? "USD";

                _logger.LogInformation($"Detected Country: {detectedCountry}, Currency: {currency}");

                return Ok(new
                {
                    country = detectedCountry,
                    currency = currency
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserLocation: {ex.Message}");
                return StatusCode(500, $"Error detecting location: {ex.Message}");
            }
        }

        private string DetectCountryFromIP(IPAddress ipAddress)
        {
            try
            {
                string databasePath = Path.Combine(AppContext.BaseDirectory, "GeoLite2-Country.mmdb");
                
                if (!System.IO.File.Exists(databasePath))
                {
                    _logger.LogWarning("GeoLite2 database file not found. Using default country.");
                    return "ZA";
                }

                if (ipAddress == null)
                {
                    _logger.LogWarning("IP address is null. Using default country.");
                    return "ZA";
                }

                if (ipAddress.ToString() == "127.0.0.1" || ipAddress.ToString() == "::1")
                {
                    _logger.LogWarning("Loopback address detected. Using default country.");
                    return "ZA";
                }

                using var reader = new DatabaseReader(databasePath);
                var country = reader.Country(ipAddress);
                return country.Country.IsoCode ?? "ZA";
            }
            catch (MaxMind.GeoIP2.Exceptions.AddressNotFoundException)
            {
                _logger.LogWarning($"IP address {ipAddress} not found in database. Using default country.");
                return "ZA";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error detecting country from IP {ipAddress}: {ex.Message}");
                return "ZA";
            }
        }

        [HttpGet("available")]
        public IActionResult GetAvailableCurrencies()
        {
            try
            {
                var currencies = new List<object>
                {
                    new { code = "ZAR", name = "South African Rand", symbol = "R" },
                    new { code = "USD", name = "US Dollar", symbol = "$" },
                    new { code = "EUR", name = "Euro", symbol = "€" },
                    new { code = "GBP", name = "British Pound", symbol = "£" },
                    new { code = "JPY", name = "Japanese Yen", symbol = "¥" },
                    new { code = "CNY", name = "Chinese Yuan", symbol = "¥" },
                    new { code = "INR", name = "Indian Rupee", symbol = "₹" },
                    new { code = "AUD", name = "Australian Dollar", symbol = "A$" },
                    new { code = "CAD", name = "Canadian Dollar", symbol = "C$" },
                    new { code = "BRL", name = "Brazilian Real", symbol = "R$" }
                };

                return Ok(currencies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAvailableCurrencies: {ex.Message}");
                return StatusCode(500, $"Error retrieving available currencies: {ex.Message}");
            }
        }

        private List<CountryCurrencyMapping> LoadCountryCurrencyMappings()
        {
            return new List<CountryCurrencyMapping>
            {
                new CountryCurrencyMapping { CountryCode = "US", CurrencyCode = "USD" },
                new CountryCurrencyMapping { CountryCode = "ZA", CurrencyCode = "ZAR" },
                new CountryCurrencyMapping { CountryCode = "GB", CurrencyCode = "GBP" },
                new CountryCurrencyMapping { CountryCode = "EU", CurrencyCode = "EUR" },
                new CountryCurrencyMapping { CountryCode = "JP", CurrencyCode = "JPY" },
                new CountryCurrencyMapping { CountryCode = "CN", CurrencyCode = "CNY" },
                new CountryCurrencyMapping { CountryCode = "IN", CurrencyCode = "INR" }
            };
        }
    }
}
