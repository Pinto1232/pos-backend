using Microsoft.AspNetCore.Mvc;
using PosBackend.DTOs.Common;

namespace PosBackend.Controllers
{
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class BaseController : ControllerBase
    {
        protected ActionResult<PagedResult<T>> PagedOk<T>(PagedResult<T> pagedResult)
        {
            // Add pagination headers
            Response.Headers["X-Total-Count"] = pagedResult.TotalRecords.ToString();
            Response.Headers["X-Page-Number"] = pagedResult.PageNumber.ToString();
            Response.Headers["X-Page-Size"] = pagedResult.PageSize.ToString();
            Response.Headers["X-Total-Pages"] = pagedResult.TotalPages.ToString();
            Response.Headers["X-Has-Next-Page"] = pagedResult.HasNextPage.ToString();
            Response.Headers["X-Has-Previous-Page"] = pagedResult.HasPreviousPage.ToString();

            return Ok(pagedResult);
        }

        protected ActionResult CachedOk<T>(T data, int cacheSeconds = 300)
        {
            // Add cache control headers
            Response.Headers["Cache-Control"] = $"public, max-age={cacheSeconds}";
            Response.Headers["ETag"] = GenerateETag(data);
            
            return Ok(data);
        }

        private static string GenerateETag<T>(T data)
        {
            var hashCode = data?.GetHashCode() ?? 0;
            return $"\"{hashCode}\"";
        }

        protected PaginationParams ValidatePaginationParams(PaginationParams pagination)
        {
            pagination.PageNumber = Math.Max(1, pagination.PageNumber);
            pagination.PageSize = Math.Max(1, Math.Min(100, pagination.PageSize));
            return pagination;
        }
    }
}