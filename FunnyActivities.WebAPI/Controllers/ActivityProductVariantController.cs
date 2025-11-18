using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Queries.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.WebAPI.Controllers.Base;
using Microsoft.Extensions.Localization;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Activity Product Variant Controller for managing activity product variants in the system.
    /// Provides comprehensive CRUD operations for activity product variant management.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations
    /// - Viewer Role: Read-only operations
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/activity-product-variants")]
    public class ActivityProductVariantController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ActivityProductVariantController> _logger;
        private readonly IInputSanitizer _inputSanitizer;
        private readonly IStringLocalizer<ActivityProductVariantController> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityProductVariantController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="inputSanitizer">The input sanitizer for security.</param>
        public ActivityProductVariantController(
            IMediator mediator,
            ILogger<ActivityProductVariantController> logger,
            IInputSanitizer inputSanitizer,
            IStringLocalizer<ActivityProductVariantController> localizer)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
            _inputSanitizer = inputSanitizer;
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieves a specific activity product variant by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the activity product variant.</param>
        /// <returns>The activity product variant information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewActivityProductVariant")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(ActivityProductVariantDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActivityProductVariant(Guid id)
        {
            _logger.LogInformation("Retrieving activity product variant with ID: {VariantId}", id);

            var query = new GetActivityProductVariantQuery { Id = id };
            var variant = await _mediator.Send(query);

            if (variant == null)
            {
                _logger.LogWarning("Activity product variant with ID {VariantId} not found", id);
                return this.ApiError(_localizer["ActivityProductVariantNotFound"], "NotFound", 404);
            }

            return this.ApiSuccess(variant, _localizer["ActivityProductVariantRetrieved"]);
        }

        /// <summary>
        /// Retrieves all activity product variants for a specific activity.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <returns>A list of activity product variants for the activity.</returns>
        [HttpGet("by-activity/{activityId}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(List<ActivityProductVariantDto>), 200)]
        public async Task<IActionResult> GetActivityProductVariantsByActivityId(Guid activityId)
        {
            _logger.LogInformation("Retrieving activity product variants for activity ID: {ActivityId}", activityId);

            var query = new GetActivityProductVariantsByActivityIdQuery { ActivityId = activityId };
            var variants = await _mediator.Send(query);

            return this.ApiSuccess(variants, _localizer["ActivityProductVariantsRetrieved"]);
        }

        /// <summary>
        /// Creates a new activity product variant.
        /// </summary>
        /// <param name="request">The activity product variant creation request.</param>
        /// <returns>The created activity product variant.</returns>
        [HttpPost]
        [Authorize(Policy = "CanCreateActivityProductVariant")]
        [ProducesResponseType(typeof(ActivityProductVariantDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateActivityProductVariant([FromBody] CreateActivityProductVariantRequest request)
        {
            _logger.LogInformation("Creating new activity product variant for activity: {ActivityId}", request.ActivityId);

            var command = new CreateActivityProductVariantCommand
            {
                ActivityId = request.ActivityId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                UnitOfMeasureId = request.UnitOfMeasureId,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity product variant created successfully with ID: {Id}", result.Id);
                return this.ApiCreated(nameof(GetActivityProductVariant), new { id = result.Id }, result, _localizer["ActivityProductVariantCreated"]);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Activity product variant creation failed: {Message}", ex.Message);
                return this.ApiError(string.Format(_localizer["ActivityProductVariantCreateValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating activity product variant");
                return this.ApiError(_localizer["ActivityProductVariantCreateUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing activity product variant.
        /// </summary>
        /// <param name="id">The unique identifier of the activity product variant to update.</param>
        /// <param name="request">The activity product variant update request.</param>
        /// <returns>The updated activity product variant.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateActivityProductVariant")]
        [ProducesResponseType(typeof(ActivityProductVariantDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateActivityProductVariant(Guid id, [FromBody] UpdateActivityProductVariantRequest request)
        {
            _logger.LogInformation("Updating activity product variant with ID: {VariantId}", id);

            var command = new UpdateActivityProductVariantCommand
            {
                Id = id,
                Quantity = request.Quantity,
                UnitOfMeasureId = request.UnitOfMeasureId,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Activity product variant updated successfully with ID: {VariantId}", result.Id);
                return this.ApiSuccess(result, _localizer["ActivityProductVariantUpdated"]);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Activity product variant update failed: {Message}", ex.Message);
                return this.ApiError(_localizer["ActivityProductVariantUpdateNotFound"], "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Activity product variant update failed: {Message}", ex.Message);
                return this.ApiError(string.Format(_localizer["ActivityProductVariantUpdateValidationError"], ex.Message), "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating activity product variant");
                return this.ApiError(_localizer["ActivityProductVariantUpdateUnexpected"], "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes an activity product variant.
        /// </summary>
        /// <param name="id">The unique identifier of the activity product variant to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteActivityProductVariant")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteActivityProductVariant(Guid id)
        {
            _logger.LogInformation("Deleting activity product variant with ID: {VariantId}", id);

            var command = new DeleteActivityProductVariantCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Activity product variant deleted successfully with ID: {VariantId}", id);
                return this.ApiSuccess<object>(_localizer["ActivityProductVariantDeleted"], 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Activity product variant deletion failed: {Message}", ex.Message);
                return this.ApiError(_localizer["ActivityProductVariantDeleteNotFound"], "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting activity product variant");
                return this.ApiError(_localizer["ActivityProductVariantDeleteUnexpected"], "InternalError", 500);
            }
        }
    }
}
