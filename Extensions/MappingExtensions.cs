using PosBackend.DTOs;
using PosBackend.Models;

namespace PosBackend.Extensions
{
    public static class MappingExtensions
    {
        // Product mapping extensions
        public static ProductListDto ToListDto(this Product product)
        {
            return new ProductListDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                BasePrice = product.BasePrice,
                CategoryName = product.Category?.Name ?? "Unknown",
                SupplierName = product.Supplier?.Name ?? "Unknown",
                VariantCount = product.VariantCount,
                HasVariants = product.HasVariants,
                CreatedAt = product.CreatedAt,
                LastUpdatedAt = product.LastUpdatedAt
            };
        }

        public static ProductDetailDto ToDetailDto(this Product product)
        {
            return new ProductDetailDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "Unknown",
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name ?? "Unknown",
                HasVariants = product.HasVariants,
                VariantCount = product.VariantCount,
                AverageVariantPrice = product.AverageVariantPrice,
                CreatedAt = product.CreatedAt,
                LastUpdatedAt = product.LastUpdatedAt,
                ProductVariants = product.ProductVariants?.Select(v => v.ToDto()).ToList() ?? new List<ProductVariantDto>()
            };
        }

        public static ProductSummaryDto ToSummaryDto(this Product product)
        {
            return new ProductSummaryDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                BasePrice = product.BasePrice
            };
        }

        public static ProductVariantDto ToDto(this ProductVariant variant)
        {
            return new ProductVariantDto
            {
                VariantId = variant.VariantId,
                Name = variant.SKU, // Using SKU as Name since ProductVariant doesn't have Name property
                Price = variant.Price,
                SKU = variant.SKU,
                StockQuantity = variant.StockQuantity
            };
        }

        public static Product ToEntity(this ProductCreateDto dto)
        {
            return new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateEntity(this ProductUpdateDto dto, Product product)
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.BasePrice = dto.BasePrice;
            product.CategoryId = dto.CategoryId;
            product.SupplierId = dto.SupplierId;
            product.LastUpdatedAt = DateTime.UtcNow;
        }

        // Customer mapping extensions
        public static CustomerListDto ToListDto(this Customer customer)
        {
            return new CustomerListDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                LoyaltyPoints = customer.LoyaltyPoint?.PointsBalance ?? 0,
                CreatedAt = customer.CreatedAt,
                LastVisit = customer.LastVisit,
                TotalOrders = customer.Sales?.Count ?? 0,
                TotalSpent = customer.Sales?.Sum(s => s.TotalAmount) ?? 0
            };
        }

        public static CustomerDetailDto ToDetailDto(this Customer customer)
        {
            return new CustomerDetailDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                DateOfBirth = customer.DateOfBirth ?? DateTime.MinValue,
                LoyaltyPoints = customer.LoyaltyPoint?.PointsBalance ?? 0,
                CreatedAt = customer.CreatedAt,
                LastVisit = customer.LastVisit,
                TotalOrders = customer.Sales?.Count ?? 0,
                TotalSpent = customer.Sales?.Sum(s => s.TotalAmount) ?? 0,
                CustomerGroups = customer.CustomerGroupMembers?.Select(cgm => new CustomerGroupSummaryDto
                {
                    GroupId = cgm.GroupId,
                    GroupName = cgm.CustomerGroup?.Name ?? "Unknown",
                    JoinedAt = cgm.JoinedAt
                }).ToList() ?? new List<CustomerGroupSummaryDto>(),
                RecentOrders = customer.Sales?.OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .Select(s => new RecentOrderDto
                    {
                        OrderId = s.SaleId,
                        OrderDate = s.SaleDate,
                        TotalAmount = s.TotalAmount,
                        Status = "Completed", // Assuming all sales are completed
                        ItemCount = s.SaleItems?.Count ?? 0
                    }).ToList() ?? new List<RecentOrderDto>()
            };
        }

        public static CustomerSummaryDto ToSummaryDto(this Customer customer)
        {
            return new CustomerSummaryDto
            {
                CustomerId = customer.CustomerId,
                FullName = $"{customer.FirstName} {customer.LastName}".Trim(),
                Email = customer.Email,
                Phone = customer.Phone
            };
        }

        public static Customer ToEntity(this CustomerCreateDto dto)
        {
            return new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                LastVisit = DateTime.UtcNow
            };
        }

        public static void UpdateEntity(this CustomerUpdateDto dto, Customer customer)
        {
            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.Address = dto.Address;
            customer.DateOfBirth = dto.DateOfBirth ?? customer.DateOfBirth;
            customer.LastVisit = DateTime.UtcNow;
        }
    }
}