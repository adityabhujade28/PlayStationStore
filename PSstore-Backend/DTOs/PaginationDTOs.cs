using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    /// <summary>
    /// Query parameters for pagination
    /// </summary>
    public class PaginationQuery
    {
        private int _pageNumber = 1;
        private int _pageSize = 20;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 100 ? 100 : (value < 1 ? 1 : value);
        }

        /// <summary>
        /// Optional search term for filtering
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Optional sorting field (e.g., "Name", "CreatedAt")
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction: "asc" or "desc"
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        public int Skip => (PageNumber - 1) * PageSize;
    }

    /// <summary>
    /// Generic paginated response wrapper
    /// </summary>
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        
        /// <summary>
        /// Total number of records in database (unfiltered)
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
        
        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        public PagedResponse() { }

        public PagedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// Specialized pagination query for games
    /// </summary>
    public class GamePaginationQuery : PaginationQuery
    {
        /// <summary>
        /// Filter by category ID
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Filter free to play only
        /// </summary>
        public bool? FreeToPlayOnly { get; set; }

        /// <summary>
        /// Include soft-deleted games
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }

    /// <summary>
    /// Specialized pagination query for users
    /// </summary>
    public class UserPaginationQuery : PaginationQuery
    {
        /// <summary>
        /// Include soft-deleted users
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;

        /// <summary>
        /// Filter by country
        /// </summary>
        public Guid? CountryId { get; set; }
    }
}
