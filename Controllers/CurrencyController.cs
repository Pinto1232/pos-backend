using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using PosBackend.Services;

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
        private readonly GeoLocationService _geoLocationService;

        public CurrencyController(ILogger<CurrencyController> logger, GeoLocationService geoLocationService)
        {
            _logger = logger;
            _geoLocationService = geoLocationService;
        }

        [HttpGet("location")]
        public IActionResult GetUserLocation()
        {
            // Log the incoming request for debugging
            _logger.LogInformation("GetUserLocation method called");

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
                string ip = ipAddress.ToString();
                string countryCode = _geoLocationService.GetCountryCode(ip);
                _logger.LogInformation($"Detected country code from IP {ip}: {countryCode}");
                return countryCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting country from IP");
                return "US"; // Default to US if detection fails
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
            // In a production environment, you might load this from a JSON or CSV file
            return new List<CountryCurrencyMapping>
            {
                new CountryCurrencyMapping { CountryCode = "US", CurrencyCode = "USD" },
                new CountryCurrencyMapping { CountryCode = "ZA", CurrencyCode = "ZAR" },
                new CountryCurrencyMapping { CountryCode = "GB", CurrencyCode = "GBP" },
                new CountryCurrencyMapping { CountryCode = "EU", CurrencyCode = "EUR" },
                new CountryCurrencyMapping { CountryCode = "JP", CurrencyCode = "JPY" },
                new CountryCurrencyMapping { CountryCode = "CN", CurrencyCode = "CNY" },
                new CountryCurrencyMapping { CountryCode = "IN", CurrencyCode = "INR" }
                // Add more country-currency mappings as needed
            };
        }
    }
}
