using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FunnyActivities.Application.Commands.FavoritesManagement;
using FunnyActivities.Application.Queries.FavoritesManagement;
using FunnyActivities.Application.DTOs.FavoritesManagement;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing user favorites with role-based authorization.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - All operations require authenticated user (automatically scoped to current user)
    /// - Users can only manage their own favorites
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/favorites")]
    [Authorize]
    public class FavoritesController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FavoritesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoritesController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public FavoritesController(IMediator mediator, ILogger<FavoritesController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Adds an activity to the user's favorites.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /api/favorites/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (201 Created):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440001",
        ///   "userId": "550e8400-e29b-41d4-a716-446655440002",
        ///   "activityId": "550e8400-e29b-41d4-a716-446655440000",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-15T10:30:00Z"
        /// }
        /// </remarks>
        /// <param name="activityId">The unique identifier of the activity to add to favorites.</param>
        /// <returns>The favorite information.</returns>
        /// <response code="201">Activity added to favorites successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="404">Activity not found</response>
        /// <response code="409">Activity is already in favorites</response>
        [HttpPost("{activityId}")]
        [ProducesResponseType(typeof(FavoritesDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> AddToFavorites(Guid activityId)
        {
            _logger.LogInformation("Adding activity {ActivityId} to favorites for user {UserId}", activityId, CurrentUserId);

            var command = new AddToFavoritesCommand
            {
                ActivityId = activityId,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity added to favorites successfully for user {UserId}", CurrentUserId);
                return this.ApiSuccess(result, "Activity added to favorites successfully", 201);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Add to favorites failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Add to favorites failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "Conflict", 409);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding activity to favorites");
                return this.ApiError("An error occurred while adding activity to favorites", "InternalError", 500);
            }
        }

        /// <summary>
        /// Removes an activity from the user's favorites.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// DELETE /api/favorites/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (204 No Content): (empty body)
        /// </remarks>
        /// <param name="activityId">The unique identifier of the activity to remove from favorites.</param>
        /// <returns>No content (successful removal).</returns>
        /// <response code="204">Activity removed from favorites successfully</response>
        /// <response code="404">Activity not found in favorites</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpDelete("{activityId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveFromFavorites(Guid activityId)
        {
            _logger.LogInformation("Removing activity {ActivityId} from favorites for user {UserId}", activityId, CurrentUserId);

            var command = new RemoveFromFavoritesCommand
            {
                ActivityId = activityId,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Activity removed from favorites successfully for user {UserId}", CurrentUserId);
                return this.ApiSuccess<object>("Activity removed from favorites successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Remove from favorites failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing activity from favorites");
                return this.ApiError("An error occurred while removing activity from favorites", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves all favorite activities for the current user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /api/favorites
        ///
        /// Sample response (200 OK):
        /// [
        ///   {
        ///     "id": "550e8400-e29b-41d4-a716-446655440000",
        ///     "userId": "550e8400-e29b-41d4-a716-446655440002",
        ///     "activityId": "550e8400-e29b-41d4-a716-446655440001",
        ///     "createdAt": "2024-01-15T10:30:00Z",
        ///     "updatedAt": "2024-01-15T10:30:00Z"
        ///   }
        /// ]
        /// </remarks>
        /// <returns>A list of all favorite activities for the current user.</returns>
        /// <response code="200">Favorites retrieved successfully</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<FavoritesDto>), 200)]
        public async Task<IActionResult> GetUserFavorites()
        {
            _logger.LogInformation("Retrieving favorites for user {UserId}", CurrentUserId);

            var query = new GetUserFavoritesQuery
            {
                UserId = CurrentUserId
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Favorites retrieved successfully");
        }

        /// <summary>
        /// Checks if a specific activity is in the user's favorites.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /api/favorites/550e8400-e29b-41d4-a716-446655440000/check
        ///
        /// Sample response (200 OK):
        /// {
        ///   "isFavorited": true
        /// }
        /// </remarks>
        /// <param name="activityId">The unique identifier of the activity to check.</param>
        /// <returns>Whether the activity is favorited by the user.</returns>
        /// <response code="200">Check completed successfully</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpGet("{activityId}/check")]
        [ProducesResponseType(typeof(CheckFavoriteResponse), 200)]
        public async Task<IActionResult> CheckFavorite(Guid activityId)
        {
            _logger.LogInformation("Checking if activity {ActivityId} is favorited by user {UserId}", activityId, CurrentUserId);

            var query = new CheckFavoriteQuery
            {
                ActivityId = activityId,
                UserId = CurrentUserId
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Favorite check completed successfully");
        }
    }
}