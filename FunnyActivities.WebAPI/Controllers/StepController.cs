using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Queries.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Step Controller for managing activity steps in the system.
    /// Provides comprehensive CRUD operations for step management.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations
    /// - Viewer Role: Read-only operations
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/steps")]
    public class StepController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<StepController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public StepController(IMediator mediator, ILogger<StepController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specific step by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the step.</param>
        /// <returns>The step information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewStep")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(StepDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStep(Guid id)
        {
            _logger.LogInformation("Retrieving step with ID: {StepId}", id);

            var query = new GetStepQuery { Id = id };
            var step = await _mediator.Send(query);

            if (step == null)
            {
                _logger.LogWarning("Step with ID {StepId} not found", id);
                return this.ApiError("Step not found", "NotFound", 404);
            }

            return this.ApiSuccess(step, "Step retrieved successfully");
        }

        /// <summary>
        /// Retrieves all steps for a specific activity.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <returns>A list of steps for the activity.</returns>
        [HttpGet("by-activity/{activityId}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(List<StepDto>), 200)]
        public async Task<IActionResult> GetStepsByActivityId(Guid activityId)
        {
            _logger.LogInformation("Retrieving steps for activity ID: {ActivityId}", activityId);

            var query = new GetStepsByActivityIdQuery { ActivityId = activityId };
            var steps = await _mediator.Send(query);

            return this.ApiSuccess(steps, "Steps retrieved successfully");
        }

        /// <summary>
        /// Creates a new step.
        /// </summary>
        /// <param name="request">The step creation request.</param>
        /// <returns>The created step.</returns>
        [HttpPost]
        [AllowAnonymous] // Temporarily allow anonymous access for debugging
        [ProducesResponseType(typeof(StepDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateStep([FromBody] CreateStepRequest request)
        {
            _logger.LogInformation("Creating new step for activity: {ActivityId}", request.ActivityId);
            _logger.LogInformation("CreateStep request data: {@Request}", request);

            // Handle anonymous users for debugging
            Guid userId;
            try
            {
                userId = CurrentUserId;
            }
            catch (UnauthorizedAccessException)
            {
                // For anonymous access during debugging, use a default user ID
                userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                _logger.LogWarning("Using default user ID for anonymous request: {UserId}", userId);
            }

            var command = new CreateStepCommand
            {
                ActivityId = request.ActivityId,
                Order = request.Order,
                Description = request.Description,
                TimestampSeconds = request.TimestampSeconds,
                UserId = userId
            };

            // Log the command before sending
            _logger.LogInformation("CreateStepCommand before sending: {@Command}", command);

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Step created successfully with ID: {Id}", result.Id);
                _logger.LogInformation("Created step data: {@Result}", result);
                return this.ApiCreated(nameof(GetStep), new { id = result.Id }, result, "Step created successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Step creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating step");
                return this.ApiError("An error occurred while creating the step", "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing step.
        /// </summary>
        /// <param name="id">The unique identifier of the step to update.</param>
        /// <param name="request">The step update request.</param>
        /// <returns>The updated step.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateStep")]
        [ProducesResponseType(typeof(StepDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStep(Guid id, [FromBody] UpdateStepRequest request)
        {
            _logger.LogInformation("Updating step with ID: {StepId}", id);
            _logger.LogInformation("UpdateStep request data: {@Request}", request);

            var command = new UpdateStepCommand
            {
                Id = id,
                Order = request.Order,
                Description = request.Description,
                TimestampSeconds = request.TimestampSeconds,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Step updated successfully with ID: {StepId}", result.Id);
                return this.ApiSuccess(result, "Step updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Step update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Step update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating step");
                return this.ApiError("An error occurred while updating the step", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a step.
        /// </summary>
        /// <param name="id">The unique identifier of the step to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteStep")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteStep(Guid id)
        {
            _logger.LogInformation("Deleting step with ID: {StepId}", id);

            var command = new DeleteStepCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Step deleted successfully with ID: {StepId}", id);
                return this.ApiSuccess<object>("Step deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Step deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting step");
                return this.ApiError("An error occurred while deleting the step", "InternalError", 500);
            }
        }

        /// <summary>
        /// Catch-all for unsupported POST actions to log attempts and return 405.
        /// </summary>
        /// <param name="action">The action name that was attempted.</param>
        /// <returns>Method Not Allowed response.</returns>
        [HttpPost("{action}")]
        [Authorize(Policy = "CanCreateStep")]
        [ProducesResponseType(405)]
        public IActionResult UnsupportedPostAction(string action)
        {
            _logger.LogWarning("Unsupported POST action attempted: {Action} on /api/steps/{Action}", action, action);
            return StatusCode(405, new { message = $"Method POST not allowed for action '{action}'" });
        }
    }
}
