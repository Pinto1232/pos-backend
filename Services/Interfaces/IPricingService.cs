using PosBackend.Models;
using PosBackend.Models.DTOs;

namespace PosBackend.Services.Interfaces
{
    public interface IPricingService
    {
        /// <summary>
        /// Get package price for specific currency
        /// </summary>
        Task<PackagePrice?> GetPackagePriceAsync(int packageId, string currency);
        
        /// <summary>
        /// Get all prices for a package
        /// </summary>
        Task<IEnumerable<PackagePrice>> GetPackagePricesAsync(int packageId);
        
        /// <summary>
        /// Get packages with pricing for specific currency
        /// </summary>
        Task<object> GetPackagesWithPricingAsync(int pageNumber, int pageSize, string currency);
        
        /// <summary>
        /// Calculate custom package price
        /// </summary>
        Task<decimal> CalculateCustomPackagePriceAsync(CustomPricingRequest request, string currency);
        
        /// <summary>
        /// Create or update package price
        /// </summary>
        Task<PackagePrice> SetPackagePriceAsync(int packageId, string currency, decimal price, DateTime? validUntil = null);
        
        /// <summary>
        /// Get package with localized pricing
        /// </summary>
        Task<PricingPackageDto?> GetPackageByIdAsync(int packageId, string currency);
    }
}