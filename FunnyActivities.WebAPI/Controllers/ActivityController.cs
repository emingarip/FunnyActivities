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
using FunnyActivities.Domain.ValueObjects;
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
        /// Test endpoint to check if anonymous access works.
        /// </summary>
        /// <returns>A simple test response.</returns>
        [HttpGet("test")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult TestAnonymous()
        {
            _logger.LogInformation("=== TEST ENDPOINT REACHED ===");
            return this.ApiSuccess(new { message = "Anonymous access works!" }, "Test successful");
        }

        /// <summary>
        /// Test endpoint to check if anonymous access works without BaseController.
        /// </summary>
        /// <returns>A simple test response.</returns>
        [HttpGet("test-simple")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult TestSimpleAnonymous()
        {
            _logger.LogInformation("=== SIMPLE TEST ENDPOINT REACHED ===");
            return Ok(new { message = "Simple anonymous access works!" });
        }

        /// <summary>
        /// Retrieves a paginated list of public activities without authentication.
        /// </summary>
        /// <remarks>
        /// Public endpoint that doesn't require authentication.
        /// Returns activities with basic information only (id, name, description, category, duration).
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 50).</param>
        /// <param name="searchTerm">Optional search term for filtering activities by name.</param>
        /// <param name="activityCategoryId">Optional activity category ID for filtering.</param>
        /// <param name="sortBy">Sort field (name, createdAt, updatedAt).</param>
        /// <param name="sortOrder">Sort order (asc, desc).</param>
        /// <returns>A paginated list of public activities.</returns>
        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<ActivityDto>), 200)]
        public async Task<IActionResult> GetPublicActivities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? activityCategoryId = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            _logger.LogInformation("=== PUBLIC ENDPOINT REACHED ===");
            _logger.LogInformation("Retrieving public activities with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

            // Validate pageSize for public endpoint (more restrictive)
            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var query = new GetActivitiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                ActivityCategoryId = activityCategoryId,
                SortBy = sortBy,
                SortOrder = sortOrder,
                IsPublic = true // Flag to indicate this is a public query
            };

            var result = await _mediator.Send(query);
            _logger.LogInformation("=== PUBLIC ENDPOINT COMPLETED ===");
            return this.ApiSuccess(result, "Public activities retrieved successfully");
        }

        /// <summary>
        /// Retrieves a specific activity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the activity.</param>
        /// <returns>The activity information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewActivity")]
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
        /// Retrieves a specific public activity by its unique identifier without authentication.
        /// </summary>
        /// <param name="id">The unique identifier of the activity.</param>
        /// <returns>The activity information if it's public.</returns>
        [HttpGet("public/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ActivityDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPublicActivity(Guid id)
        {
            _logger.LogInformation("=== PUBLIC ACTIVITY ENDPOINT REACHED ===");
            _logger.LogInformation("Retrieving public activity with ID: {ActivityId}", id);

            var query = new GetActivityQuery { Id = id, IsPublicRequest = true };
            var activity = await _mediator.Send(query);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found or not public", id);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            _logger.LogInformation("=== PUBLIC ACTIVITY RETRIEVED SUCCESSFULLY ===");
            return this.ApiSuccess(activity, "Public activity retrieved successfully");
        }

        /// <summary>
        /// Retrieves an activity with all its associated details including steps and product variants.
        /// </summary>
        /// <param name="id">The unique identifier of the activity.</param>
        /// <returns>The activity with its details.</returns>
        [HttpGet("{id}/with-details")]
        [Authorize(Policy = "CanViewActivity")]
        [ProducesResponseType(typeof(ActivityWithDetailsDto), 200)]
        [ProducesResponseType(404)]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
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
                IsPublic = request.IsPublic,
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
        /// <param name="activityId">The activity ID.</param>
        /// <param name="videoFile">The video file to upload.</param>
        /// <returns>The upload result with signed URL for video access.</returns>
        [HttpPost("{activityId}/upload-video")]
        [Authorize(Policy = "CanUpdateActivity")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadActivityVideoResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UploadActivityVideo(Guid activityId, [FromForm(Name = "videoData")] IFormFile videoFile)
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
                // Validate video file
                if (videoFile == null || videoFile.Length == 0)
                {
                    return this.ApiError("No video file provided", "ValidationError", 400);
                }

                // Convert IFormFile to byte array
                using var memoryStream = new MemoryStream();
                await videoFile.CopyToAsync(memoryStream);
                var videoData = memoryStream.ToArray();

                // Upload video to Minio
                var objectKey = await _minioService.UploadVideoAsync(videoData, videoFile.FileName, videoFile.ContentType, activityId);
                _logger.LogInformation("Video uploaded to MinIO with object key: {ObjectKey}", objectKey);

                // Update activity's VideoUrl with the object key
                var updateCommand = new UpdateActivityCommand
                {
                    Id = activityId,
                    Name = activity.Name, // Keep existing values
                    Description = activity.Description,
                    VideoUrl = objectKey, // Set VideoUrl to object key
                    // Duration fields are optional, keep existing values
                    UserId = CurrentUserId
                };

                _logger.LogInformation("Updating activity {ActivityId} with video object key: {ObjectKey}", activityId, objectKey);

                await _mediator.Send(updateCommand);

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

                _logger.LogInformation("Video uploaded and activity updated successfully for activity ID: {ActivityId}, Object Key: {ObjectKey}", activityId, objectKey);
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
        /// Gets a signed URL for accessing an activity's video (public endpoint).
        /// </summary>
        /// <remarks>
        /// Public endpoint that doesn't require authentication.
        /// Returns a signed URL for accessing the activity's video if the activity exists.
        /// </remarks>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <param name="videoObjectKey">The video object key in storage.</param>
        /// <param name="expirySeconds">The expiry time for the signed URL in seconds (default: 3600).</param>
        /// <returns>The signed URL for video access.</returns>
        [HttpGet("public/{activityId}/video-url")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPublicActivityVideoUrl(Guid activityId, [FromQuery] string videoObjectKey, [FromQuery] int expirySeconds = 3600)
        {
            _logger.LogInformation("=== PUBLIC VIDEO URL ENDPOINT REACHED ===");
            _logger.LogInformation("Generating public video URL for activity ID: {ActivityId}, Object Key: {ObjectKey}", activityId, videoObjectKey);

            // Validate input parameters
            if (string.IsNullOrWhiteSpace(videoObjectKey))
            {
                _logger.LogWarning("Video object key is null or empty for activity ID: {ActivityId}", activityId);
                return this.ApiError("Video object key is required", "ValidationError", 400);
            }

            // Validate expiry seconds
            if (expirySeconds <= 0 || expirySeconds > 86400) // Max 24 hours
            {
                _logger.LogWarning("Invalid expiry seconds: {ExpirySeconds} for activity ID: {ActivityId}", expirySeconds, activityId);
                expirySeconds = 3600; // Default to 1 hour
            }

            // Validate that the activity exists
            var activityQuery = new GetActivityQuery { Id = activityId };
            var activity = await _mediator.Send(activityQuery);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found for public video URL request", activityId);
                return this.ApiError("Activity not found", "NotFound", 404);
            }

            // TODO: Add IsPublic property to Activity entity and validate here
            // For now, we assume the activity is public if it exists
            _logger.LogInformation("Activity found: {ActivityName} (ID: {ActivityId}). Assuming public access.", activity.Name, activityId);

            try
            {
                // Generate signed URL for video access
                var signedUrl = await _minioService.GenerateVideoPreSignedUrlAsync(videoObjectKey, expirySeconds);

                var response = new
                {
                    ActivityId = activityId,
                    ActivityName = activity.Name,
                    VideoObjectKey = videoObjectKey,
                    SignedVideoUrl = signedUrl,
                    UrlExpirySeconds = expirySeconds,
                    GeneratedAt = DateTime.UtcNow,
                    IsPublicAccess = true
                };

                _logger.LogInformation("Successfully generated public video URL for activity ID: {ActivityId}, Object Key: {ObjectKey}", activityId, videoObjectKey);
                return this.ApiSuccess(response, "Public video URL generated successfully");
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning("Video object not found: {ObjectKey} for activity ID: {ActivityId}. Error: {ErrorMessage}", videoObjectKey, activityId, ex.Message);
                return this.ApiError("Video not found", "VideoNotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "MinIO service error while generating public video URL for activity ID: {ActivityId}, Object Key: {ObjectKey}", activityId, videoObjectKey);
                return this.ApiError("Unable to generate video URL. Storage service unavailable.", "StorageServiceError", 503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while generating public video URL for activity ID: {ActivityId}, Object Key: {ObjectKey}", activityId, videoObjectKey);
                return this.ApiError("An error occurred while generating the video URL", "InternalError", 500);
            }
        }

        /// <summary>
        /// Gets metadata for a video object using GET-based methods.
        /// </summary>
        /// <remarks>
        /// This endpoint provides video metadata (size, content type, last modified date, etc.)
        /// using GET-based requests instead of HEAD requests, which can be more reliable
        /// in certain network configurations.
        /// </remarks>
        /// <param name="videoObjectKey">The video object key in storage.</param>
        /// <returns>The video metadata information.</returns>
        [HttpGet("video-metadata")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ObjectMetadata), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetVideoMetadata([FromQuery] string videoObjectKey)
        {
            _logger.LogInformation("Retrieving video metadata for object key: {ObjectKey}", videoObjectKey);

            // Validate input parameters
            if (string.IsNullOrWhiteSpace(videoObjectKey))
            {
                _logger.LogWarning("Video object key is null or empty");
                return this.ApiError("Video object key is required", "ValidationError", 400);
            }

            try
            {
                var metadata = await _minioService.GetVideoMetadataAsync(videoObjectKey);

                _logger.LogInformation("Successfully retrieved metadata for video object: {ObjectKey}", videoObjectKey);
                return this.ApiSuccess(metadata, "Video metadata retrieved successfully");
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning("Video object not found: {ObjectKey}. Error: {ErrorMessage}", videoObjectKey, ex.Message);
                return this.ApiError("Video not found", "VideoNotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "MinIO service error while retrieving video metadata for object: {ObjectKey}", videoObjectKey);
                return this.ApiError("Unable to retrieve video metadata. Storage service unavailable.", "StorageServiceError", 503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving video metadata for object: {ObjectKey}", videoObjectKey);
                return this.ApiError("An error occurred while retrieving video metadata", "InternalError", 500);
            }
        }

        /// <summary>
        /// Gets metadata for any object using GET-based methods.
        /// </summary>
        /// <remarks>
        /// This endpoint provides object metadata (size, content type, last modified date, etc.)
        /// using GET-based requests instead of HEAD requests, which can be more reliable
        /// in certain network configurations.
        /// </remarks>
        /// <param name="objectKey">The object key in storage.</param>
        /// <returns>The object metadata information.</returns>
        [HttpGet("object-metadata")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ObjectMetadata), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetObjectMetadata([FromQuery] string objectKey)
        {
            _logger.LogInformation("Retrieving object metadata for object key: {ObjectKey}", objectKey);

            // Validate input parameters
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                _logger.LogWarning("Object key is null or empty");
                return this.ApiError("Object key is required", "ValidationError", 400);
            }

            try
            {
                var metadata = await _minioService.GetObjectMetadataAsync(objectKey);

                _logger.LogInformation("Successfully retrieved metadata for object: {ObjectKey}", objectKey);
                return this.ApiSuccess(metadata, "Object metadata retrieved successfully");
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning("Object not found: {ObjectKey}. Error: {ErrorMessage}", objectKey, ex.Message);
                return this.ApiError("Object not found", "ObjectNotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "MinIO service error while retrieving object metadata for object: {ObjectKey}", objectKey);
                return this.ApiError("Unable to retrieve object metadata. Storage service unavailable.", "StorageServiceError", 503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving object metadata for object: {ObjectKey}", objectKey);
                return this.ApiError("An error occurred while retrieving object metadata", "InternalError", 500);
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