using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.ValueObjects;
using FunnyActivities.CrossCuttingConcerns.Caching;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for updating an existing activity.
    /// </summary>
    public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateActivityCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivityCommandHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="cache">The cache service.</param>
        /// <param name="logger">The logger.</param>
        public UpdateActivityCommandHandler(IActivityRepository activityRepository, ICacheService cache, ILogger<UpdateActivityCommandHandler> logger)
        {
            _activityRepository = activityRepository;
            _cache = cache;
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
                throw new KeyNotFoundException($"The activity with ID {request.Id} could not be found. Please verify the activity exists and try again.");
            }

            _logger.LogInformation("Current activity data - Name: {Name}, VideoUrl: {VideoUrl}, Duration: {Duration}",
                activity.Name, activity.VideoUrl?.Value, activity.Duration?.ToString());

            // Create value objects
            VideoUrl? videoUrl = null;
            if (request.VideoUrl != null)
            {
                // VideoUrl is provided in the request (can be empty string to clear)
                if (!string.IsNullOrWhiteSpace(request.VideoUrl))
                {
                    videoUrl = VideoUrl.Create(request.VideoUrl);
                    _logger.LogInformation("Setting VideoUrl to: {VideoUrl}", request.VideoUrl);
                }
                else
                {
                    // Empty string provided - explicitly clear the video URL
                    _logger.LogInformation("VideoUrl is empty string in request - clearing the existing video URL");
                }
                // videoUrl remains null, which will clear the existing video URL
            }
            else
            {
                // VideoUrl is null in request - don't update the video URL, keep existing
                _logger.LogInformation("VideoUrl is null in request - keeping existing video URL unchanged");
                videoUrl = activity.VideoUrl; // Keep existing value
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

            // Determine intro video changes
            VideoUrl? introVideoUrl = activity.IntroVideoUrl;
            if (request.IntroVideoUrl != null)
            {
                if (!string.IsNullOrWhiteSpace(request.IntroVideoUrl))
                {
                    introVideoUrl = VideoUrl.Create(request.IntroVideoUrl);
                    _logger.LogInformation("Setting IntroVideoUrl to: {IntroVideoUrl}", request.IntroVideoUrl);
                }
                else
                {
                    introVideoUrl = null;
                    _logger.LogInformation("IntroVideoUrl set to empty string - clearing intro video");
                }
            }

            // Update the activity
            activity.UpdateDetails(request.Name, request.Description, videoUrl, duration, request.IsPublic);

            if (request.IntroVideoUrl != null)
            {
                activity.UpdateIntroVideo(introVideoUrl);
            }

            // Save to repository
            await _activityRepository.UpdateAsync(activity);

            // Invalidate public activities cache since activity might have changed public status
            await InvalidatePublicActivitiesCacheAsync();

            _logger.LogInformation("Activity updated successfully with ID: {ActivityId}, IsPublic: {IsPublic}", request.Id, request.IsPublic);

            // Map to DTO
            var activityDto = new ActivityDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                VideoUrl = activity.VideoUrl?.Value,
                IntroVideoUrl = activity.IntroVideoUrl?.Value,
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

        private async Task InvalidatePublicActivitiesCacheAsync()
        {
            try
            {
                // Clear all public activities cache keys (simplified approach)
                // In a production system, you might want to use a pattern-based invalidation
                // For now, we'll clear a few common cache keys
                var commonCacheKeys = new[]
                {
                    "public_activities_1_10_name_asc",
                    "public_activities_1_20_name_asc",
                    "public_activities_1_50_name_asc"
                };

                foreach (var key in commonCacheKeys)
                {
                    await _cache.RemoveAsync(key);
                }

                _logger.LogInformation("Invalidated public activities cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating public activities cache");
            }
        }
    }
}
