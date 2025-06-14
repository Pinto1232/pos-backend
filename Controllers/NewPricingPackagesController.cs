using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PosBackend.Application.Services.Caching;
using PosBackend.Models;
using PosBackend.Models.DTOs;
using PosBackend.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/v2/pricing-packages")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NewPricingPackagesController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly ICurrencyDetectionService _currencyDetection;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<NewPricingPackagesController> _logger;

        public NewPricingPackagesController(
            IPricingService pricingService,
            ICurrencyDetectionService currencyDetection,
            ICurrencyService currencyService,
            ILogger<NewPricingPackagesController> logger)
        {
            _pricingService = pricingService;
            _currencyDetection = currencyDetection;
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all pricing packages with localized pricing
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAll(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? currency = null)
        {
            try
            {
                var userCurrency = currency ?? await _currencyDetection.DetectCurrencyAsync(HttpContext);
                
                if (!_currencyService.IsCurrencySupported(userCurrency))
                {
                    return BadRequest($"Currency '{userCurrency}' is not supported");
                }

                var result = await _pricingService.GetPackagesWithPricingAsync(pageNumber, pageSize, userCurrency);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pricing packages");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific pricing package by ID with localized pricing
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PricingPackageDto>> GetById(
            int id,
            [FromQuery] string? currency = null)
        {
            try
            {
                var userCurrency = currency ?? await _currencyDetection.DetectCurrencyAsync(HttpContext);
                
                if (!_currencyService.IsCurrencySupported(userCurrency))
                {
                    return BadRequest($"Currency '{userCurrency}' is not supported");
                }

                var package = await _pricingService.GetPackageByIdAsync(id, userCurrency);
                
                if (package == null)
                {
                    return NotFound($"Pricing package with ID {id} not found");
                }

                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pricing package {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all prices for a specific package
        /// </summary>
        [HttpGet("{id}/prices")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PackagePriceDto>>> GetPackagePrices(int id)
        {
            try
            {
                var prices = await _pricingService.GetPackagePricesAsync(id);
                
                var priceDtos = prices.Select(p => new PackagePriceDto
                {
                    PackageId = p.PackageId,
                    Currency = p.Currency,
                    Price = p.Price,
                    CreatedAt = p.CreatedAt,
                    ValidUntil = p.ValidUntil
                }).ToList();

                return Ok(priceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prices for package {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate custom package price with localized currency
        /// </summary>
        [HttpPost("custom/calculate-price")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CalculateCustomPrice(
            [FromBody] CustomPricingRequest request,
            [FromQuery] string? currency = null)
        {
            try
            {
                var userCurrency = currency ?? await _currencyDetection.DetectCurrencyAsync(HttpContext);
                
                if (!_currencyService.IsCurrencySupported(userCurrency))
                {
                    return BadRequest($"Currency '{userCurrency}' is not supported");
                }

                var totalPrice = await _pricingService.CalculateCustomPackagePriceAsync(request, userCurrency);
                var basePrice = await _pricingService.GetPackagePriceAsync(request.PackageId, userCurrency);

                return Ok(new { 
                    basePrice = basePrice?.Price ?? 0,
                    totalPrice = totalPrice,
                    currency = userCurrency
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating custom package price");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Set package price for specific currency (Admin only)
        /// </summary>
        [HttpPost("{id}/prices")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PackagePriceDto>> SetPackagePrice(
            int id,
            [FromBody] SetPackagePriceRequest request)
        {
            try
            {
                if (!_currencyService.IsCurrencySupported(request.Currency))
                {
                    return BadRequest($"Currency '{request.Currency}' is not supported");
                }

                var packagePrice = await _pricingService.SetPackagePriceAsync(
                    id, 
                    request.Currency, 
                    request.Price, 
                    request.ValidUntil);

                var dto = new PackagePriceDto
                {
                    PackageId = packagePrice.PackageId,
                    Currency = packagePrice.Currency,
                    Price = packagePrice.Price,
                    CreatedAt = packagePrice.CreatedAt,
                    ValidUntil = packagePrice.ValidUntil
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting package price for package {PackageId}", id);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Convert price between currencies
        /// </summary>
        [HttpPost("convert")]
        [AllowAnonymous]
        public async Task<ActionResult<CurrencyConversionResult>> ConvertCurrency(
            [FromBody] CurrencyConversionRequest request)
        {
            try
            {
                if (!_currencyService.IsCurrencySupported(request.FromCurrency))
                {
                    return BadRequest($"Source currency '{request.FromCurrency}' is not supported");
                }

                if (!_currencyService.IsCurrencySupported(request.ToCurrency))
                {
                    return BadRequest($"Target currency '{request.ToCurrency}' is not supported");
                }

                var result = await _currencyService.ConvertWithDetailsAsync(
                    request.Amount, 
                    request.FromCurrency, 
                    request.ToCurrency);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency from {FromCurrency} to {ToCurrency}", 
                    request.FromCurrency, request.ToCurrency);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get supported currencies
        /// </summary>
        [HttpGet("currencies")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Currency>>> GetSupportedCurrencies()
        {
            try
            {
                var currencies = await _currencyService.GetSupportedCurrenciesAsync();
                return Ok(currencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supported currencies");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get exchange rates for a base currency
        /// </summary>
        [HttpGet("exchange-rates/{baseCurrency}")]
        [AllowAnonymous]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetExchangeRates(string baseCurrency)
        {
            try
            {
                if (!_currencyService.IsCurrencySupported(baseCurrency))
                {
                    return BadRequest($"Currency '{baseCurrency}' is not supported");
                }

                var rates = await _currencyService.GetExchangeRatesAsync(baseCurrency);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rates for {BaseCurrency}", baseCurrency);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Set user currency preference
        /// </summary>
        [HttpPost("user/currency-preference")]
        public async Task<IActionResult> SetUserCurrencyPreference([FromBody] SetCurrencyPreferenceRequest request)
        {
            try
            {
                var userId = HttpContext.User.FindFirst("sub")?.Value ?? 
                            HttpContext.User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found");
                }

                if (!_currencyService.IsCurrencySupported(request.Currency))
                {
                    return BadRequest($"Currency '{request.Currency}' is not supported");
                }

                await _currencyDetection.SetUserPreferredCurrencyAsync(userId, request.Currency);

                return Ok(new { message = "Currency preference updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user currency preference");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class SetPackagePriceRequest
    {
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public DateTime? ValidUntil { get; set; }
    }

    public class CurrencyConversionRequest
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string FromCurrency { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string ToCurrency { get; set; } = string.Empty;
    }

    public class SetCurrencyPreferenceRequest
    {
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;
    }
}