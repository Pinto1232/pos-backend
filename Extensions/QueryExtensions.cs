using Microsoft.EntityFrameworkCore;
using PosBackend.DTOs.Common;
using System.Linq.Expressions;

namespace PosBackend.Extensions
{
    public static class QueryExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PaginationParams pagination)
        {
            var totalRecords = await query.CountAsync();
            
            var data = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> query,
            string? searchTerm,
            Expression<Func<T, bool>> searchExpression)
        {
            return string.IsNullOrWhiteSpace(searchTerm) 
                ? query 
                : query.Where(searchExpression);
        }

        public static IQueryable<T> ApplyOrdering<T>(
            this IQueryable<T> query,
            string? sortBy,
            string? sortOrder)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var propertyInfo = typeof(T).GetProperty(sortBy);
            if (propertyInfo == null)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = sortOrder?.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), propertyInfo.PropertyType },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        // Optimized include methods for specific entities
        public static IQueryable<Models.Product> IncludeBasicProductInfo(this IQueryable<Models.Product> query)
        {
            return query
                .Include(p => p.Category)
                .Include(p => p.Supplier);
        }

        public static IQueryable<Models.Product> IncludeFullProductInfo(this IQueryable<Models.Product> query)
        {
            return query
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductVariants);
        }

        public static IQueryable<Models.Customer> IncludeBasicCustomerInfo(this IQueryable<Models.Customer> query)
        {
            return query
                .Include(c => c.LoyaltyPoint);
        }

        public static IQueryable<Models.Customer> IncludeFullCustomerInfo(this IQueryable<Models.Customer> query)
        {
            return query
                .Include(c => c.LoyaltyPoint)
                .Include(c => c.CustomerGroupMembers)
                    .ThenInclude(cgm => cgm.CustomerGroup)
                .Include(c => c.Sales.Take(5)) // Only recent 5 sales for performance
                    .ThenInclude(s => s.SaleItems);
        }
    }
}