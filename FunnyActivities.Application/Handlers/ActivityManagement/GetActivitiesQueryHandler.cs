using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
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
        private readonly IMinioService _minioService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivitiesQueryHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="minioService">The Minio service for generating signed URLs.</param>
        public GetActivitiesQueryHandler(IActivityRepository activityRepository, ILogger<GetActivitiesQueryHandler> logger, IMinioService minioService)
        {
            _activityRepository = activityRepository;
            _logger = logger;
            _minioService = minioService;
        }

        /// <summary>
        /// Determines if a video URL is a MinIO object key that needs signed URL conversion.
        /// </summary>
        /// <param name="videoUrl">The video URL to check.</param>
        /// <returns>True if the URL is a MinIO object key, false otherwise.</returns>
        private bool IsMinioObjectKey(string videoUrl)
        {
            // MinIO object keys for videos start with "videos/" pattern
            // They are not valid HTTP/HTTPS URLs
            return !string.IsNullOrEmpty(videoUrl) &&
                   videoUrl.StartsWith("videos/") &&
                   !Uri.TryCreate(videoUrl, UriKind.Absolute, out _);
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

            // Get filtered and paginated activities from the database
            var (activities, totalCount) = await _activityRepository.GetFilteredAsync(
                request.SearchTerm,
                request.ActivityCategoryId,
                request.IsPublic,
                request.SortBy,
                request.SortOrder,
                request.PageNumber,
                request.PageSize);

            // Map to DTOs and process video URLs
            var activityDtos = new List<ActivityDto>();
            foreach (var activity in activities)
            {
                string videoUrl = null;

                // Check if the activity has a video URL and if it's a MinIO object key
                if (activity.VideoUrl != null && IsMinioObjectKey(activity.VideoUrl.Value))
                {
                    try
                    {
                        // Check if MinIO service is available before attempting to generate signed URL
                        if (_minioService != null)
                        {
                            // Generate signed URL for MinIO object key
                            videoUrl = await _minioService.GenerateVideoPreSignedUrlAsync(activity.VideoUrl.Value);
                            _logger.LogInformation("Generated signed URL for video object key: {ObjectKey}", activity.VideoUrl.Value);
                        }
                        else
                        {
                            _logger.LogWarning("MinIO service is not available for object key: {ObjectKey}", activity.VideoUrl.Value);
                            // Fallback to object key if MinIO service is not available
                            videoUrl = activity.VideoUrl.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate signed URL for video object key: {ObjectKey}", activity.VideoUrl.Value);
                        // Fallback to object key if signed URL generation fails
                        videoUrl = activity.VideoUrl.Value;
                    }
                }
                else
                {
                    // Use the original URL if it's not a MinIO object key
                    videoUrl = activity.VideoUrl?.Value;
                }

                activityDtos.Add(new ActivityDto
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    Description = activity.Description,
                    VideoUrl = videoUrl,
                    Duration = activity.Duration?.ToString(),
                    ActivityCategoryId = activity.ActivityCategoryId,
                    ActivityCategoryName = activity.ActivityCategory?.Name ?? "Unknown",
                    CreatedAt = activity.CreatedAt,
                    UpdatedAt = activity.UpdatedAt,
                    StepCount = activity.Steps?.Count ?? 0,
                    ProductVariantCount = activity.ActivityProductVariants?.Count ?? 0
                });
            }

            var result = new PagedResult<ActivityDto>(activityDtos, request.PageNumber, request.PageSize, totalCount);

            _logger.LogInformation("Retrieved {Count} activities out of {TotalCount} total", activityDtos.Count, totalCount);

            return result;
        }
    }
}