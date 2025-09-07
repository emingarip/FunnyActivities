using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using FunnyActivities.Application.Commands.UnitOfMeasureManagement;
using FunnyActivities.Application.Queries.UnitOfMeasureManagement;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing units of measure with role-based authorization.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations (Create, Read, Update, Delete)
    /// - Viewer Role: Read-only operations (Get single unit, List units)
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/units-of-measure")]
    public class UnitOfMeasureController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UnitOfMeasureController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfMeasureController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public UnitOfMeasureController(IMediator mediator, ILogger<UnitOfMeasureController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new unit of measure in the system.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        ///
        /// Sample request:
        /// POST /api/units-of-measure
        /// {
        ///   "name": "Kilogram",
        ///   "symbol": "kg",
        ///   "type": "Weight"
        /// }
        ///
        /// Sample response (201 Created):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440001",
        ///   "name": "Kilogram",
        ///   "symbol": "kg",
        ///   "type": "Weight",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-15T10:30:00Z"
        /// }
        /// </remarks>
        /// <param name="request">The unit of measure creation request containing all required information.</param>
        /// <returns>The complete unit of measure information including the generated ID and timestamps.</returns>
        /// <response code="201">Unit of measure created successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpPost]
        [Authorize(Policy = "CanCreateUnit")]
        [ProducesResponseType(typeof(UnitOfMeasureDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateUnitOfMeasure([FromBody] CreateUnitOfMeasureRequest request)
        {
            _logger.LogInformation("Creating new unit of measure: {Name}", request.Name);

            var command = new CreateUnitOfMeasureCommand
            {
                Name = request.Name,
                Symbol = request.Symbol,
                Type = request.Type
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Unit of measure created successfully with ID: {Id}", result.Id);
                return this.ApiSuccess(result, "Unit of measure created successfully", 201);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Unit of measure creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating unit of measure");
                return this.ApiError("An error occurred while creating the unit of measure", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves a specific unit of measure by its unique identifier.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        ///
        /// Sample request:
        /// GET /api/units-of-measure/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (200 OK):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440000",
        ///   "name": "Kilogram",
        ///   "symbol": "kg",
        ///   "type": "Weight",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-20T14:45:00Z"
        /// }
        /// </remarks>
        /// <param name="id">The unique identifier (GUID) of the unit of measure to retrieve.</param>
        /// <returns>The complete unit of measure information if found.</returns>
        /// <response code="200">Unit of measure found and returned successfully</response>
        /// <response code="404">Unit of measure not found with the specified ID</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin or Viewer role required</response>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewUnit")]
        [ProducesResponseType(typeof(UnitOfMeasureDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUnitOfMeasure(Guid id)
        {
            _logger.LogInformation("Retrieving unit of measure with ID: {UnitId}", id);

            var query = new GetUnitOfMeasureQuery { Id = id };
            var unit = await _mediator.Send(query);

            if (unit == null)
            {
                _logger.LogWarning("Unit of measure with ID {UnitId} not found", id);
                return this.ApiError("Unit of measure not found", "NotFound", 404);
            }

            return this.ApiSuccess(unit, "Unit of measure retrieved successfully");
        }

        /// <summary>
        /// Updates an existing unit of measure with new information.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        /// Only provided fields will be updated; omitted fields retain their current values.
        ///
        /// Sample request:
        /// PUT /api/units-of-measure/550e8400-e29b-41d4-a716-446655440000
        /// {
        ///   "name": "Updated Kilogram",
        ///   "symbol": "kg",
        ///   "type": "Weight"
        /// }
        ///
        /// Sample response (200 OK):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440000",
        ///   "name": "Updated Kilogram",
        ///   "symbol": "kg",
        ///   "type": "Weight",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-20T15:30:00Z"
        /// }
        /// </remarks>
        /// <param name="id">The unique identifier (GUID) of the unit of measure to update.</param>
        /// <param name="request">The unit of measure update request containing fields to modify.</param>
        /// <returns>The complete updated unit of measure information.</returns>
        /// <response code="200">Unit of measure updated successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="404">Unit of measure not found with the specified ID</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateUnit")]
        [ProducesResponseType(typeof(UnitOfMeasureDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUnitOfMeasure(Guid id, [FromBody] UpdateUnitOfMeasureRequest request)
        {
            _logger.LogInformation("Updating unit of measure with ID: {UnitId}", id);

            var command = new UpdateUnitOfMeasureCommand
            {
                Id = id,
                Name = request.Name,
                Symbol = request.Symbol,
                Type = request.Type
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Unit of measure updated successfully with ID: {UnitId}", result.Id);
                return this.ApiSuccess(result, "Unit of measure updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Unit of measure update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Unit of measure update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating unit of measure");
                return this.ApiError("An error occurred while updating the unit of measure", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a unit of measure from the system.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        /// This operation permanently removes the unit of measure and cannot be undone.
        ///
        /// Sample request:
        /// DELETE /api/units-of-measure/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (204 No Content): (empty body)
        /// </remarks>
        /// <param name="id">The unique identifier (GUID) of the unit of measure to delete.</param>
        /// <returns>No content (successful deletion).</returns>
        /// <response code="204">Unit of measure deleted successfully</response>
        /// <response code="404">Unit of measure not found with the specified ID</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteUnit")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUnitOfMeasure(Guid id)
        {
            _logger.LogInformation("Deleting unit of measure with ID: {UnitId}", id);

            var command = new DeleteUnitOfMeasureCommand
            {
                Id = id
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Unit of measure deleted successfully with ID: {UnitId}", id);
                return this.ApiSuccess<object>("Unit of measure deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Unit of measure deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting unit of measure");
                return this.ApiError("An error occurred while deleting the unit of measure", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves a list of all units of measure.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        ///
        /// Sample request:
        /// GET /api/units-of-measure
        ///
        /// Sample response (200 OK):
        /// [
        ///   {
        ///     "id": "550e8400-e29b-41d4-a716-446655440000",
        ///     "name": "Kilogram",
        ///     "symbol": "kg",
        ///     "type": "Weight",
        ///     "createdAt": "2024-01-15T10:30:00Z",
        ///     "updatedAt": "2024-01-15T10:30:00Z"
        ///   }
        /// ]
        /// </remarks>
        /// <returns>A list of all units of measure.</returns>
        /// <response code="200">Units of measure list retrieved successfully</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin or Viewer role required</response>
        [HttpGet]
        [Authorize(Policy = "CanViewUnit")]
        [ProducesResponseType(typeof(List<UnitOfMeasureDto>), 200)]
        public async Task<IActionResult> GetUnitsOfMeasure()
        {
            _logger.LogInformation("Retrieving all units of measure");

            var query = new GetUnitOfMeasuresQuery();
            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Units of measure retrieved successfully");
        }
    }
}