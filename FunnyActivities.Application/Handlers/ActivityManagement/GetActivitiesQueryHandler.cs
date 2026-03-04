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

        private bool TryGetObjectKey(string? value, out string objectKey)
        {
            objectKey = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // Direct object key (older records)
            if (value.StartsWith("videos/") && !Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                objectKey = value;
                return true;
            }

            // Signed URL that contains bucket/object path
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath.TrimStart('/');
                const string bucketPrefix = "activity-videos/";
                if (path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    objectKey = path.Substring(bucketPrefix.Length);
                    return true;
                }
            }

            return false;
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
                string introVideoUrl = null;

                // Check if the activity has a video URL and if it's a MinIO object key
                if (TryGetObjectKey(activity.VideoUrl?.Value, out var objectKey))
                {
                    try
                    {
                        // Check if MinIO service is available before attempting to generate signed URL
                        if (_minioService != null)
                        {
                            // Generate signed URL for MinIO object key
                            videoUrl = await _minioService.GenerateVideoPreSignedUrlAsync(objectKey);
                            _logger.LogInformation("Generated signed URL for video object key: {ObjectKey}", objectKey);
                        }
                        else
                        {
                            _logger.LogWarning("MinIO service is not available for object key: {ObjectKey}", objectKey);
                            // Fallback to object key if MinIO service is not available
                            videoUrl = objectKey;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate signed URL for video object key: {ObjectKey}", objectKey);
                        // Fallback to object key if signed URL generation fails
                        videoUrl = objectKey;
                    }
                }
                else
                {
                    // Use the original URL if it's not a MinIO object key
                    videoUrl = activity.VideoUrl?.Value;
                }

                // Resolve intro video URL if available
                if (TryGetObjectKey(activity.IntroVideoUrl?.Value, out var introObjectKey))
                {
                    try
                    {
                        if (_minioService != null)
                        {
                            introVideoUrl = await _minioService.GenerateVideoPreSignedUrlAsync(introObjectKey);
                            _logger.LogInformation("Generated signed URL for intro video object key: {ObjectKey}", introObjectKey);
                        }
                        else
                        {
                            _logger.LogWarning("MinIO service is not available for intro object key: {ObjectKey}", introObjectKey);
                            introVideoUrl = introObjectKey;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate signed URL for intro video object key: {ObjectKey}", introObjectKey);
                        introVideoUrl = introObjectKey;
                    }
                }
                else
                {
                    introVideoUrl = activity.IntroVideoUrl?.Value;
                }

                activityDtos.Add(new ActivityDto
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    Description = activity.Description,
                    VideoUrl = videoUrl,
                    IntroVideoUrl = introVideoUrl,
                    Duration = activity.Duration?.ToString(),
                    ActivityCategoryId = activity.ActivityCategoryId,
                    ActivityCategoryName = activity.ActivityCategory?.Name ?? "Unknown",
                    IsPublic = activity.IsPublic,
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
