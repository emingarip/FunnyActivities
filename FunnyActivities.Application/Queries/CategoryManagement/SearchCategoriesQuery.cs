using MediatR;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Queries.CategoryManagement
{
    /// <summary>
    /// Query for searching categories based on a search term.
    /// </summary>
    public class SearchCategoriesQuery : IRequest<PagedResult<CategoryDto>>
    {
        /// <summary>
        /// Gets or sets the search term.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the page number (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}