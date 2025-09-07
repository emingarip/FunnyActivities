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
    /// Handler for retrieving a paginated list of activities.
    /// </summary>
    public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, PagedResult<ActivityDto>>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<GetActivitiesQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivitiesQueryHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public GetActivitiesQueryHandler(IActivityRepository activityRepository, ILogger<GetActivitiesQueryHandler> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get activities query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paginated result of activities.</returns>
        public async Task<PagedResult<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving activities with page: {PageNumber}, pageSize: {PageSize}", request.PageNumber, request.PageSize);

            // Get all activities (in a real implementation, you'd want a more efficient repository method)
            var allActivities = await _activityRepository.GetAllAsync();

            // Apply filtering
            var filteredActivities = allActivities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                filteredActivities = filteredActivities.Where(a => a.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                                 (a.Description != null && a.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (request.ActivityCategoryId.HasValue)
            {
                filteredActivities = filteredActivities.Where(a => a.ActivityCategoryId == request.ActivityCategoryId.Value);
            }

            // Apply sorting
            filteredActivities = request.SortBy?.ToLower() switch
            {
                "name" => request.SortOrder?.ToLower() == "desc"
                    ? filteredActivities.OrderByDescending(a => a.Name)
                    : filteredActivities.OrderBy(a => a.Name),
                "createdat" => request.SortOrder?.ToLower() == "desc"
                    ? filteredActivities.OrderByDescending(a => a.CreatedAt)
                    : filteredActivities.OrderBy(a => a.CreatedAt),
                "updatedat" => request.SortOrder?.ToLower() == "desc"
                    ? filteredActivities.OrderByDescending(a => a.UpdatedAt)
                    : filteredActivities.OrderBy(a => a.UpdatedAt),
                _ => filteredActivities.OrderBy(a => a.Name)
            };

            // Apply pagination
            var totalCount = filteredActivities.Count();
            var activities = filteredActivities
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var activityDtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                VideoUrl = a.VideoUrl?.Value,
                Duration = a.Duration?.ToString(),
                ActivityCategoryId = a.ActivityCategoryId,
                ActivityCategoryName = a.ActivityCategory?.Name ?? "Unknown",
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                StepCount = a.Steps?.Count ?? 0,
                ProductVariantCount = a.ActivityProductVariants?.Count ?? 0
            });

            var result = new PagedResult<ActivityDto>(activityDtos, request.PageNumber, request.PageSize, totalCount);

            _logger.LogInformation("Retrieved {Count} activities out of {TotalCount} total", activities.Count, totalCount);

            return result;
        }
    }
}