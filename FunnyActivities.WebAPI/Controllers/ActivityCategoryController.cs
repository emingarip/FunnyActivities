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
using FunnyActivities.Domain.Exceptions;
using FunnyActivities.WebAPI.Controllers.Base;
using Microsoft.Extensions.Localization;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Activity Category Controller for managing activity categories in the system.
    /// Provides comprehensive CRUD operations for activity category management.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations
    /// - Viewer Role: Read-only operations
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/activity-categories")]
    public class ActivityCategoryController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ActivityCategoryController> _logger;
        private readonly IInputSanitizer _inputSanitizer;
        private readonly IStringLocalizer<ActivityCategoryController> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCategoryController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="inputSanitizer">The input sanitizer for security.</param>
        public ActivityCategoryController(
            IMediator mediator,
            ILogger<ActivityCategoryController> logger,
            IInputSanitizer inputSanitizer,
            IStringLocalizer<ActivityCategoryController> localizer)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
            _inputSanitizer = inputSanitizer;
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieves a paginated list of activity categories with optional filtering.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        /// Returns activity categories with their activity counts.
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="searchTerm">Optional search term for filtering categories by name.</param>
        /// <param name="sortBy">Sort field (name, createdAt, updatedAt).</param>
        /// <param name="sortOrder">Sort order (asc, desc).</param>
        /// <returns>A paginated list of activity categories.</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewActivityCategory")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(PagedResult<ActivityCategoryDto>), 200)]
        public async Task<IActionResult> GetActivityCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            _logger.LogInformation("Retrieving activity categories with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new GetActivityCategoriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, _localizer["ActivityCategoriesRetrieved"]);
        }

        /// <summary>
        /// Retrieves a specific activity category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the activity category.</param>
        /// <returns>The activity category information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewActivityCategory")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(ActivityCategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActivityCategory(Guid id)
        {
            _logger.LogInformation("Retrieving activity category with ID: {CategoryId}", id);

            var query = new GetActivityCategoryQuery { Id = id };
            var category = await _mediator.Send(query);

            if (category == null)
            {
                _logger.LogWarning("Activity category with ID {CategoryId} not found", id);
                return this.ApiError(_localizer["ActivityCategoryNotFound"], "NotFound", 404);
            }

            return this.ApiSuccess(category, _localizer["ActivityCategoryRetrieved"]);
        }

        /// <summary>
        /// Creates a new activity category.
        /// </summary>
        /// <param name="request">The activity category creation request.</param>
        /// <returns>The created activity category.</returns>
        [HttpPost]
        [Authorize(Policy = "CanCreateActivityCategory")]
        [ProducesResponseType(typeof(ActivityCategoryDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateActivityCategory([FromBody] CreateActivityCategoryRequest request)
        {
            _logger.LogInformation("Creating new activity category: {Name}", request.Name);

            // Sanitize user inputs
            var sanitizedName = _inputSanitizer.SanitizeString(request.Name, 200);
            var sanitizedDescription = request.Description != null ? _inputSanitizer.SanitizeString(request.Description, 1000) : null;

            var command = new CreateActivityCategoryCommand
            {
                Name = sanitizedName,
                Description = sanitizedDescription,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity category created successfully with ID: {Id}", result.Id);
                return this.ApiCreated(nameof(GetActivityCategory), new { id = result.Id }, result, _localizer["ActivityCategoryCreated"]);
            }
            catch (ActivityCategoryNameAlreadyExistsException ex)
            {
                _logger.LogWarning("Activity category creation failed: {Message}", ex.Message);
                return this.ApiError(
                    string.Format(_localizer["ActivityCategoryAlreadyExists"], request.Name),
                    "Conflict",
                    409);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Activity category creation failed: {Message}", ex.Message);
                return this.ApiError(string.Format(_localizer["ActivityCategoryCreateValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating activity category");
                return this.ApiError(_localizer["ActivityCategoryCreateUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing activity category.
        /// </summary>
        /// <param name="id">The unique identifier of the activity category to update.</param>
        /// <param name="request">The activity category update request.</param>
        /// <returns>The updated activity category.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateActivityCategory")]
        [ProducesResponseType(typeof(ActivityCategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateActivityCategory(Guid id, [FromBody] UpdateActivityCategoryRequest request)
        {
            _logger.LogInformation("Updating activity category with ID: {CategoryId}", id);

            // Sanitize user inputs
            var sanitizedName = request.Name != null ? _inputSanitizer.SanitizeString(request.Name, 200) : null;
            var sanitizedDescription = request.Description != null ? _inputSanitizer.SanitizeString(request.Description, 1000) : null;

            var command = new UpdateActivityCategoryCommand
            {
                Id = id,
                Name = sanitizedName,
                Description = sanitizedDescription,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity category updated successfully with ID: {CategoryId}", result.Id);
                return this.ApiSuccess(result, _localizer["ActivityCategoryUpdated"]);
            }
            catch (ActivityCategoryNameAlreadyExistsException ex)
            {
                _logger.LogWarning("Activity category update failed: {Message}", ex.Message);
                return this.ApiError(
                    string.Format(_localizer["ActivityCategoryAlreadyExists"], request.Name),
                    "Conflict",
                    409);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Activity category update failed: {Message}", ex.Message);
                return this.ApiError(_localizer["ActivityCategoryUpdateNotFound"], "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Activity category update failed: {Message}", ex.Message);
                return this.ApiError(string.Format(_localizer["ActivityCategoryUpdateValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating activity category");
                return this.ApiError(_localizer["ActivityCategoryUpdateUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes an activity category.
        /// </summary>
        /// <param name="id">The unique identifier of the activity category to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteActivityCategory")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteActivityCategory(Guid id)
        {
            _logger.LogInformation("Deleting activity category with ID: {CategoryId}", id);

            var command = new DeleteActivityCategoryCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Activity category deleted successfully with ID: {CategoryId}", id);
                return this.ApiSuccess<object>(_localizer["ActivityCategoryDeleted"], 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Activity category deletion failed: {Message}", ex.Message);
                return this.ApiError(_localizer["ActivityCategoryDeleteNotFound"], "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting activity category");
                return this.ApiError(_localizer["ActivityCategoryDeleteUnexpected"], "InternalError", 500);
            }
        }
    }
}
