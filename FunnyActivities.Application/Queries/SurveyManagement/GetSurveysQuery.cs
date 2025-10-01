using MediatR;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Queries.SurveyManagement
{
    /// <summary>
    /// Query for getting a list of surveys.
    /// </summary>
    public class GetSurveysQuery : IRequest<SurveyListResult>
    {
        /// <summary>
        /// Gets or sets the ID of the user making the request (optional, for filtering user's surveys).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include only active surveys.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include only currently active surveys (within date range).
        /// </summary>
        public bool? IsCurrentlyActive { get; set; }

        /// <summary>
        /// Gets or sets the search term for filtering surveys by title or description.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the page number for pagination.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Gets or sets a value indicating whether to sort in descending order.
        /// </summary>
        public bool SortDescending { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveysQuery"/> class.
        /// </summary>
        public GetSurveysQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSurveysQuery"/> class with specified parameters.
        /// </summary>
        /// <param name="userId">The ID of the user making the request.</param>
        /// <param name="isActive">Whether to include only active surveys.</param>
        /// <param name="isCurrentlyActive">Whether to include only currently active surveys.</param>
        /// <param name="searchTerm">The search term for filtering.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The page size for pagination.</param>
        /// <param name="sortBy">The sort field.</param>
        /// <param name="sortDescending">Whether to sort in descending order.</param>
        public GetSurveysQuery(Guid? userId = null, bool? isActive = null, bool? isCurrentlyActive = null, string searchTerm = null, int pageNumber = 1, int pageSize = 10, string sortBy = "CreatedAt", bool sortDescending = true)
        {
            UserId = userId;
            IsActive = isActive;
            IsCurrentlyActive = isCurrentlyActive;
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortBy = sortBy;
            SortDescending = sortDescending;
        }
    }
}