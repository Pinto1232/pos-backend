using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PosBackend.Models;
using PosBackend.Models.DTOs;
using PosBackend.Services;
using PosBackend.Services.Interfaces;
using System.Security.Claims;
using Xunit;

namespace PosBackend.Tests
{
    public class PricingBestPracticesTests
    {
        private static PosDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            return new PosDbContext(options);
        }
        [Fact]
        public async Task CurrencyDetectionService_Should_Detect_Currency_From_Query_Parameter()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockCache = new Mock<PosBackend.Application.Services.Caching.ICacheService>();
            var mockGeoService = new Mock<PosBackend.Services.GeoLocationService>("test");
            var mockLogger = new Mock<ILogger<CurrencyDetectionService>>();
            var options = Options.Create(new PricingOptions { 
                DefaultCurrency = "USD",
                SupportedCurrencies = new[] { "USD", "EUR", "GBP", "ZAR" }
            });

            var service = new CurrencyDetectionService(
                context,
                mockCache.Object,
                mockGeoService.Object,
                mockLogger.Object,
                options);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "currency", "EUR" }
            });

            // Act
            var result = await service.DetectCurrencyAsync(httpContext);

            // Assert
            Assert.Equal("EUR", result);
        }

        [Fact]
        public void CurrencyDetectionService_Should_Map_Country_To_Currency()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockCache = new Mock<PosBackend.Application.Services.Caching.ICacheService>();
            var mockGeoService = new Mock<PosBackend.Services.GeoLocationService>("test");
            var mockLogger = new Mock<ILogger<CurrencyDetectionService>>();
            var options = Options.Create(new PricingOptions { 
                DefaultCurrency = "USD",
                SupportedCurrencies = new[] { "USD", "EUR", "GBP", "ZAR", "CAD", "AUD", "JPY" }
            });

            var service = new CurrencyDetectionService(
                context,
                mockCache.Object,
                mockGeoService.Object,
                mockLogger.Object,
                options);

            // Act & Assert
            Assert.Equal("USD", service.MapCountryToCurrency("US"));
            Assert.Equal("GBP", service.MapCountryToCurrency("GB"));
            Assert.Equal("EUR", service.MapCountryToCurrency("DE"));
            Assert.Equal("ZAR", service.MapCountryToCurrency("ZA"));
            Assert.Equal("USD", service.MapCountryToCurrency("XX")); // Unknown country
        }

        [Fact]
        public void CurrencyDetectionService_Should_Extract_Currency_From_Language()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockCache = new Mock<PosBackend.Application.Services.Caching.ICacheService>();
            var mockGeoService = new Mock<PosBackend.Services.GeoLocationService>("test");
            var mockLogger = new Mock<ILogger<CurrencyDetectionService>>();
            var options = Options.Create(new PricingOptions { 
                DefaultCurrency = "USD",
                SupportedCurrencies = new[] { "USD", "EUR", "GBP", "ZAR" }
            });

            var service = new CurrencyDetectionService(
                context,
                mockCache.Object,
                mockGeoService.Object,
                mockLogger.Object,
                options);

            // Act & Assert
            Assert.Equal("USD", service.GetCurrencyFromLanguage("en-US,en;q=0.9"));
            Assert.Equal("GBP", service.GetCurrencyFromLanguage("en-GB,en;q=0.9"));
            Assert.Equal("EUR", service.GetCurrencyFromLanguage("de-DE,de;q=0.8,en;q=0.7"));
            Assert.Null(service.GetCurrencyFromLanguage("zh-CN")); // Not supported
            Assert.Null(service.GetCurrencyFromLanguage("")); // Empty
            Assert.Null(service.GetCurrencyFromLanguage(null)); // Null
        }

        [Theory]
        [InlineData("USD", true)]
        [InlineData("EUR", true)]
        [InlineData("GBP", true)]
        [InlineData("ZAR", true)]
        [InlineData("XYZ", false)]
        [InlineData("", false)]
        public void CurrencyService_Should_Validate_Supported_Currencies(string currency, bool expected)
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockCache = new Mock<PosBackend.Application.Services.Caching.ICacheService>();
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var options = Options.Create(new PricingOptions 
            { 
                SupportedCurrencies = new[] { "USD", "EUR", "GBP", "ZAR" }
            });

            var service = new CurrencyService(
                context,
                mockHttpClientFactory.Object,
                mockCache.Object,
                mockLogger.Object,
                options);

            // Act
            var result = service.IsCurrencySupported(currency);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PricingOptions_Should_Have_Default_Values()
        {
            // Arrange & Act
            var options = new PricingOptions();

            // Assert
            Assert.Equal("USD", options.DefaultCurrency);
            Assert.Equal(TimeSpan.FromHours(1), options.CacheExchangeRatesFor);
            Assert.Null(options.SupportedCurrencies);
        }

        [Fact]
        public void PackagePrice_Should_Validate_Required_Fields()
        {
            // Arrange & Act
            var packagePrice = new PackagePrice
            {
                PackageId = 1,
                Currency = "USD",
                Price = 29.99m,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, packagePrice.PackageId);
            Assert.Equal("USD", packagePrice.Currency);
            Assert.Equal(29.99m, packagePrice.Price);
            Assert.True(packagePrice.CreatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Currency_Should_Have_Correct_Properties()
        {
            // Arrange & Act
            var currency = new Currency
            {
                Code = "USD",
                Name = "US Dollar",
                Symbol = "$",
                IsActive = true,
                DecimalPlaces = 2,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal("USD", currency.Code);
            Assert.Equal("US Dollar", currency.Name);
            Assert.Equal("$", currency.Symbol);
            Assert.True(currency.IsActive);
            Assert.Equal(2, currency.DecimalPlaces);
        }

        [Fact]
        public void ExchangeRate_Should_Store_Conversion_Data()
        {
            // Arrange & Act
            var exchangeRate = new ExchangeRate
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Rate = 0.93m,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Assert
            Assert.Equal("USD", exchangeRate.FromCurrency);
            Assert.Equal("EUR", exchangeRate.ToCurrency);
            Assert.Equal(0.93m, exchangeRate.Rate);
            Assert.NotNull(exchangeRate.ExpiresAt);
        }

        [Fact]
        public void CurrencyConversionResult_Should_Track_Conversion_Details()
        {
            // Arrange & Act
            var result = new CurrencyConversionResult
            {
                OriginalAmount = 100.00m,
                FromCurrency = "USD",
                ConvertedAmount = 93.00m,
                ToCurrency = "EUR",
                ExchangeRate = 0.93m,
                ConvertedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(100.00m, result.OriginalAmount);
            Assert.Equal("USD", result.FromCurrency);
            Assert.Equal(93.00m, result.ConvertedAmount);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(0.93m, result.ExchangeRate);
        }
    }
}