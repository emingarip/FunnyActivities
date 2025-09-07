using MediatR;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Queries.CategoryManagement
{
    /// <summary>
    /// Query for retrieving a paginated list of categories.
    /// </summary>
    public class GetCategoriesQuery : IRequest<PagedResult<CategoryDto>>
    {
        /// <summary>
        /// Gets or sets the page number (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the search term for filtering categories.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the field to sort by.
        /// </summary>
        public string? SortBy { get; set; } = "name";

        /// <summary>
        /// Gets or sets the sort order (asc or desc).
        /// </summary>
        public string? SortOrder { get; set; } = "asc";
    }
}