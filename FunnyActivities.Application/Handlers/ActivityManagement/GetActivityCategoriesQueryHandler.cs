using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.ActivityManagement;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for retrieving a paginated list of activity categories.
    /// </summary>
    public class GetActivityCategoriesQueryHandler : IRequestHandler<GetActivityCategoriesQuery, PagedResult<ActivityCategoryDto>>
    {
        private readonly IActivityCategoryRepository _activityCategoryRepository;
        private readonly ILogger<GetActivityCategoriesQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivityCategoriesQueryHandler"/> class.
        /// </summary>
        /// <param name="activityCategoryRepository">The activity category repository.</param>
        /// <param name="logger">The logger.</param>
        public GetActivityCategoriesQueryHandler(IActivityCategoryRepository activityCategoryRepository, ILogger<GetActivityCategoriesQueryHandler> logger)
        {
            _activityCategoryRepository = activityCategoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get activity categories query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paginated result of activity categories.</returns>
        public async Task<PagedResult<ActivityCategoryDto>> Handle(GetActivityCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving activity categories with page: {PageNumber}, pageSize: {PageSize}", request.PageNumber, request.PageSize);

            // Get all activity categories
            var allCategories = await _activityCategoryRepository.GetAllAsync();

            // Apply filtering
            var filteredCategories = allCategories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                filteredCategories = filteredCategories.Where(c => c.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                                 (c.Description != null && c.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            filteredCategories = request.SortBy?.ToLower() switch
            {
                "name" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCategories.OrderByDescending(c => c.Name)
                    : filteredCategories.OrderBy(c => c.Name),
                "createdat" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCategories.OrderByDescending(c => c.CreatedAt)
                    : filteredCategories.OrderBy(c => c.CreatedAt),
                "updatedat" => request.SortOrder?.ToLower() == "desc"
                    ? filteredCategories.OrderByDescending(c => c.UpdatedAt)
                    : filteredCategories.OrderBy(c => c.UpdatedAt),
                _ => filteredCategories.OrderBy(c => c.Name)
            };

            // Apply pagination
            var totalCount = filteredCategories.Count();
            var categories = filteredCategories
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var categoryDtos = categories.Select(c => new ActivityCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ActivityCount = c.Activities?.Count ?? 0
            });

            var result = new PagedResult<ActivityCategoryDto>(categoryDtos, request.PageNumber, request.PageSize, totalCount);

            _logger.LogInformation("Retrieved {Count} activity categories out of {TotalCount} total", categories.Count, totalCount);

            return result;
        }
    }
}