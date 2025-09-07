using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Queries.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Activity Controller for managing activities in the system.
    /// Provides comprehensive CRUD operations for activity management.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations
    /// - Viewer Role: Read-only operations
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/activities")]
    public class ActivityController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ActivityController> _logger;
        private readonly IMinioService _minioService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="minioService">The Minio service for file operations.</param>
        public ActivityController(IMediator mediator, ILogger<ActivityController> logger, IMinioService minioService)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
            _minioService = minioService;
        }

        /// <summary>
        /// Retrieves a paginated list of activities with optional filtering.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        /// Returns activities with their category information.
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="searchTerm">Optional search term for filtering activities by name.</param>
        /// <param name="activityCategoryId">Optional activity category ID for filtering.</param>
        /// <param name="sortBy">Sort field (name, createdAt, updatedAt).</param>
        /// <param name="sortOrder">Sort order (asc, desc).</param>
        /// <returns>A paginated list of activities.</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewActivity")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(PagedResult<ActivityDto>), 200)]
        public async Task<IActionResult> GetActivities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? activityCategoryId = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            _logger.LogInformation("Retrieving activities with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new GetActivitiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                ActivityCategoryId = activityCategoryId,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Activities retrieved successfully");
        }

        /// <summary>
        /// Retrieves a specific activity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the activity.</param>
        /// <returns>The activity information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewActivity")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(ActivityDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActivity(Guid id)
        {
            _logger.LogInformation("Retrieving activity with ID: {ActivityId}", id);

            var query = new GetActivityQuery { Id = id };
            var activity = await _mediator.Send(query);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", id);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            return this.ApiSuccess(activity, "Activity retrieved successfully");
        }

        /// <summary>
        /// Retrieves an activity with all its associated details including steps and product variants.
        /// </summary>
        /// <param name="id">The unique identifier of the activity.</param>
        /// <returns>The activity with its details.</returns>
        [HttpGet("{id}/with-details")]
        [Authorize(Policy = "CanViewActivity")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(ActivityWithDetailsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActivityWithDetails(Guid id)
        {
            _logger.LogInformation("Retrieving activity with details for ID: {ActivityId}", id);

            var query = new GetActivityWithDetailsQuery { Id = id };
            var activity = await _mediator.Send(query);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", id);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            return this.ApiSuccess(activity, "Activity with details retrieved successfully");
        }

        /// <summary>
        /// Creates a new activity.
        /// </summary>
        /// <param name="request">The activity creation request.</param>
        /// <returns>The created activity.</returns>
        [HttpPost]
        [Authorize(Policy = "CanCreateActivity")]
        [ProducesResponseType(typeof(ActivityDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request)
        {
            _logger.LogInformation("Creating new activity: {Name}", request.Name);

            var command = new CreateActivityCommand
            {
                Name = request.Name,
                Description = request.Description,
                VideoUrl = request.VideoUrl,
                DurationHours = request.DurationHours,
                DurationMinutes = request.DurationMinutes,
                DurationSeconds = request.DurationSeconds,
                ActivityCategoryId = request.ActivityCategoryId,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity created successfully with ID: {Id}", result.Id);
                return this.ApiCreated(nameof(GetActivity), new { id = result.Id }, result, "Activity created successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Activity creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating activity");
                return this.ApiError("An error occurred while creating the activity", "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing activity.
        /// </summary>
        /// <param name="id">The unique identifier of the activity to update.</param>
        /// <param name="request">The activity update request.</param>
        /// <returns>The updated activity.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateActivity")]
        [ProducesResponseType(typeof(ActivityDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] UpdateActivityRequest request)
        {
            _logger.LogInformation("Updating activity with ID: {ActivityId}", id);

            var command = new UpdateActivityCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                VideoUrl = request.VideoUrl,
                DurationHours = request.DurationHours,
                DurationMinutes = request.DurationMinutes,
                DurationSeconds = request.DurationSeconds,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity updated successfully with ID: {ActivityId}", result.Id);
                return this.ApiSuccess(result, "Activity updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Activity update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Activity update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating activity");
                return this.ApiError("An error occurred while updating the activity", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes an activity.
        /// </summary>
        /// <param name="id">The unique identifier of the activity to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteActivity")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            _logger.LogInformation("Deleting activity with ID: {ActivityId}", id);

            var command = new DeleteActivityCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Activity deleted successfully with ID: {ActivityId}", id);
                return this.ApiSuccess<object>("Activity deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Activity deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting activity");
                return this.ApiError("An error occurred while deleting the activity", "InternalError", 500);
            }
        }

        /// <summary>
        /// Uploads a video for a specific activity.
        /// </summary>
        /// <param name="request">The video upload request containing activity ID and video data.</param>
        /// <returns>The upload result with signed URL for video access.</returns>
        [HttpPost("{activityId}/upload-video")]
        [Authorize(Policy = "CanUpdateActivity")]
        [ProducesResponseType(typeof(UploadActivityVideoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UploadActivityVideo(Guid activityId, [FromBody] UploadActivityVideoRequest request)
        {
            _logger.LogInformation("Uploading video for activity ID: {ActivityId}", activityId);

            // Validate that the activity exists
            var activityQuery = new GetActivityQuery { Id = activityId };
            var activity = await _mediator.Send(activityQuery);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", activityId);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            try
            {
                // Upload video to Minio
                var objectKey = await _minioService.UploadVideoAsync(request.VideoData, request.FileName, request.ContentType, activityId);

                // Generate signed URL for video access
                var signedUrl = await _minioService.GenerateVideoPreSignedUrlAsync(objectKey);

                var response = new UploadActivityVideoResponse
                {
                    ActivityId = activityId,
                    VideoObjectKey = objectKey,
                    SignedVideoUrl = signedUrl,
                    UrlExpirySeconds = 3600, // 1 hour
                    UploadedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Video uploaded successfully for activity ID: {ActivityId}, Object Key: {ObjectKey}", activityId, objectKey);
                return this.ApiSuccess(response, "Video uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading video for activity {ActivityId}", activityId);
                return this.ApiError("An error occurred while uploading the video", "InternalError", 500);
            }
        }

        /// <summary>
        /// Gets a signed URL for accessing an activity's video.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <param name="videoObjectKey">The video object key in storage.</param>
        /// <param name="expirySeconds">The expiry time for the signed URL in seconds (default: 3600).</param>
        /// <returns>The signed URL for video access.</returns>
        [HttpGet("{activityId}/video-url")]
        [Authorize(Policy = "CanViewActivity")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActivityVideoUrl(Guid activityId, [FromQuery] string videoObjectKey, [FromQuery] int expirySeconds = 3600)
        {
            _logger.LogInformation("Generating video URL for activity ID: {ActivityId}", activityId);

            // Validate that the activity exists
            var activityQuery = new GetActivityQuery { Id = activityId };
            var activity = await _mediator.Send(activityQuery);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", activityId);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            try
            {
                var signedUrl = await _minioService.GenerateVideoPreSignedUrlAsync(videoObjectKey, expirySeconds);

                var response = new
                {
                    ActivityId = activityId,
                    VideoObjectKey = videoObjectKey,
                    SignedVideoUrl = signedUrl,
                    UrlExpirySeconds = expirySeconds,
                    GeneratedAt = DateTime.UtcNow
                };

                return this.ApiSuccess(response, "Video URL generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating video URL for activity {ActivityId}", activityId);
                return this.ApiError("An error occurred while generating the video URL", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a video for a specific activity.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <param name="videoObjectKey">The video object key in storage.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{activityId}/video")]
        [Authorize(Policy = "CanUpdateActivity")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteActivityVideo(Guid activityId, [FromQuery] string videoObjectKey)
        {
            _logger.LogInformation("Deleting video for activity ID: {ActivityId}", activityId);

            // Validate that the activity exists
            var activityQuery = new GetActivityQuery { Id = activityId };
            var activity = await _mediator.Send(activityQuery);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", activityId);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            try
            {
                var deleted = await _minioService.DeleteVideoAsync(videoObjectKey);

                if (!deleted)
                {
                    _logger.LogWarning("Failed to delete video for activity {ActivityId}, Object Key: {ObjectKey}", activityId, videoObjectKey);
                    return this.ApiError("Failed to delete the video", "DeletionFailed", 500);
                }

                _logger.LogInformation("Video deleted successfully for activity ID: {ActivityId}", activityId);
                return this.ApiSuccess<object>("Video deleted successfully", 204);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting video for activity {ActivityId}", activityId);
                return this.ApiError("An error occurred while deleting the video", "InternalError", 500);
            }
        }
    }
}