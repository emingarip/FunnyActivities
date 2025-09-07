using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Queries.ActivityManagement
{
    /// <summary>
    /// Query for retrieving a paginated list of activities.
    /// </summary>
    public class GetActivitiesQuery : IRequest<PagedResult<ActivityDto>>
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
        /// Gets or sets the search term for filtering activities.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the activity category ID for filtering.
        /// </summary>
        public Guid? ActivityCategoryId { get; set; }

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