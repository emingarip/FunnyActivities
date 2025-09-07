using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for creating a new activity.
    /// </summary>
    public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<CreateActivityCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateActivityCommandHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public CreateActivityCommandHandler(IActivityRepository activityRepository, ILogger<CreateActivityCommandHandler> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create activity command.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created activity DTO.</returns>
        public async Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new activity: {Name}", request.Name);

            // Create value objects
            VideoUrl? videoUrl = null;
            if (!string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                videoUrl = VideoUrl.Create(request.VideoUrl);
            }

            Duration? duration = null;
            if (request.DurationHours.HasValue || request.DurationMinutes.HasValue || request.DurationSeconds.HasValue)
            {
                var hours = request.DurationHours ?? 0;
                var minutes = request.DurationMinutes ?? 0;
                var seconds = request.DurationSeconds ?? 0;
                duration = Duration.Create(hours, minutes, seconds);
            }

            // Create the activity
            var activity = Activity.Create(request.Name, request.Description, videoUrl, duration, request.ActivityCategoryId);

            // Save to repository
            await _activityRepository.AddAsync(activity);

            _logger.LogInformation("Activity created successfully with ID: {Id}", activity.Id);

            // Map to DTO
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

            return activityDto;
        }
    }
}