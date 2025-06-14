using System.Collections.Generic;

namespace PosBackend.DTOs.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
        
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
    }
}