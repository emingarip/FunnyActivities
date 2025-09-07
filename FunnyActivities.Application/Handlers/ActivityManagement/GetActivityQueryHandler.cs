using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.ActivityManagement;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for retrieving a single activity by ID.
    /// </summary>
    public class GetActivityQueryHandler : IRequestHandler<GetActivityQuery, ActivityDto?>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<GetActivityQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivityQueryHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public GetActivityQueryHandler(IActivityRepository activityRepository, ILogger<GetActivityQueryHandler> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get activity query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The activity DTO or null if not found.</returns>
        public async Task<ActivityDto?> Handle(GetActivityQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving activity with ID: {ActivityId}", request.Id);

            var activity = await _activityRepository.GetByIdAsync(request.Id);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", request.Id);
                return null;
            }

            var activityDto = new ActivityDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                VideoUrl = activity.VideoUrl?.Value,
                Duration = activity.Duration?.ToString(),
                ActivityCategoryId = activity.ActivityCategoryId,
                ActivityCategoryName = activity.ActivityCategory?.Name ?? "Unknown",
                CreatedAt = activity.CreatedAt,
                UpdatedAt = activity.UpdatedAt,
                StepCount = activity.Steps?.Count ?? 0,
                ProductVariantCount = activity.ActivityProductVariants?.Count ?? 0
            };

            _logger.LogInformation("Successfully retrieved activity with ID: {ActivityId}", request.Id);

            return activityDto;
        }
    }
}