using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for updating an existing activity.
    /// </summary>
    public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<UpdateActivityCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivityCommandHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public UpdateActivityCommandHandler(IActivityRepository activityRepository, ILogger<UpdateActivityCommandHandler> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update activity command.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated activity DTO.</returns>
        public async Task<ActivityDto> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating activity with ID: {ActivityId}", request.Id);
            _logger.LogInformation("Update request data - Name: {Name}, Description: {Description}, VideoUrl: {VideoUrl}, DurationHours: {DurationHours}, DurationMinutes: {DurationMinutes}, DurationSeconds: {DurationSeconds}",
                request.Name, request.Description, request.VideoUrl, request.DurationHours, request.DurationMinutes, request.DurationSeconds);

            var activity = await _activityRepository.GetByIdAsync(request.Id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", request.Id);
                throw new KeyNotFoundException($"Activity with ID {request.Id} not found");
            }

            _logger.LogInformation("Current activity data - Name: {Name}, VideoUrl: {VideoUrl}, Duration: {Duration}",
                activity.Name, activity.VideoUrl?.Value, activity.Duration?.ToString());

            // Create value objects
            VideoUrl? videoUrl = null;
            if (!string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                videoUrl = VideoUrl.Create(request.VideoUrl);
                _logger.LogInformation("Setting VideoUrl to: {VideoUrl}", request.VideoUrl);
            }
            else
            {
                _logger.LogWarning("VideoUrl is null/empty in request - this will clear the existing video!");
            }

            Duration? duration = null;
            if (request.DurationHours.HasValue || request.DurationMinutes.HasValue || request.DurationSeconds.HasValue)
            {
                var hours = request.DurationHours ?? 0;
                var minutes = request.DurationMinutes ?? 0;
                var seconds = request.DurationSeconds ?? 0;
                duration = Duration.Create(hours, minutes, seconds);
                _logger.LogInformation("Setting Duration to: {Duration} (from {Hours}:{Minutes}:{Seconds})",
                    duration.ToString(), hours, minutes, seconds);
            }
            else
            {
                _logger.LogWarning("No duration values provided in request - this will clear the existing duration!");
            }

            // Update the activity
            activity.UpdateDetails(request.Name, request.Description, videoUrl, duration, request.IsPublic);

            // Save to repository
            await _activityRepository.UpdateAsync(activity);

            _logger.LogInformation("Activity updated successfully with ID: {ActivityId}, IsPublic: {IsPublic}", request.Id, request.IsPublic);

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