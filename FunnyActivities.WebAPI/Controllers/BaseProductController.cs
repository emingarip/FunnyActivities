using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Application.Queries.BaseProductManagement;
using FunnyActivities.Application.DTOs.BaseProductManagement;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing base products with role-based authorization.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations (Create, Read, Update, Delete)
    /// - Viewer Role: Read-only operations (Get single base product, List base products)
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/base-products")]
    public class BaseProductController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BaseProductController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProductController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public BaseProductController(IMediator mediator, ILogger<BaseProductController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new base product in the system.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        ///
        /// Sample request:
        /// POST /api/base-products
        /// {
        ///   "name": "Laptop Computer",
        ///   "description": "High-performance laptop for business use",
        ///   "categoryId": "550e8400-e29b-41d4-a716-446655440000"
        /// }
        ///
        /// Sample response (201 Created):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440001",
        ///   "name": "Laptop Computer",
        ///   "description": "High-performance laptop for business use",
        ///   "categoryId": "550e8400-e29b-41d4-a716-446655440000",
        ///   "categoryName": "Electronics",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-15T10:30:00Z"
        /// }
        /// </remarks>
        /// <param name="request">The base product creation request containing all required base product information.</param>
        /// <returns>The complete base product information including the generated ID and timestamps.</returns>
        /// <response code="201">Base product created successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpPost]
        [Authorize(Policy = "CanCreateBaseProduct")]
        [ProducesResponseType(typeof(BaseProductDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateBaseProduct([FromBody] CreateBaseProductRequest request)
        {
            _logger.LogInformation("Creating new base product: {Name}", request.Name);

            var command = new CreateBaseProductCommand
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Base product created successfully with ID: {Id}", result.Id);
                return this.ApiSuccess(result, "Base product created successfully", 201);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Base product creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating base product");
                return this.ApiError("An error occurred while creating the base product", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves a specific base product by its unique identifier.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        ///
        /// Sample request:
        /// GET /api/base-products/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (200 OK):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440000",
        ///   "name": "Laptop Computer",
        ///   "description": "High-performance laptop for business use",
        ///   "categoryId": "550e8400-e29b-41d4-a716-446655440000",
        ///   "categoryName": "Electronics",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-20T14:45:00Z"
        /// }
        /// </remarks>
        /// <param name="id">The unique identifier (GUID) of the base product to retrieve.</param>
        /// <returns>The complete base product information if found.</returns>
        /// <response code="200">Base product found and returned successfully</response>
        /// <response code="404">Base product not found with the specified ID</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin or Viewer role required</response>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewBaseProduct")]
        [ProducesResponseType(typeof(BaseProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBaseProduct(Guid id)
        {
            _logger.LogInformation("Retrieving base product with ID: {BaseProductId}", id);

            var query = new GetBaseProductQuery { Id = id };
            var baseProduct = await _mediator.Send(query);

            if (baseProduct == null)
            {
                _logger.LogWarning("Base product with ID {BaseProductId} not found", id);
                return this.ApiError("Base product not found", "NotFound", 404);
            }

            return this.ApiSuccess(baseProduct, "Base product retrieved successfully");
        }

        /// <summary>
        /// Updates an existing base product with new information.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        /// Only provided fields will be updated; omitted fields retain their current values.
        ///
        /// Sample request:
        /// PUT /api/base-products/550e8400-e29b-41d4-a716-446655440000
        /// {
        ///   "name": "Updated Laptop Computer",
        ///   "description": "Updated description",
        ///   "categoryId": "550e8400-e29b-41d4-a716-446655440001"
        /// }
        ///
        /// Sample response (200 OK):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440000",
        ///   "name": "Updated Laptop Computer",
        ///   "description": "Updated description",
        ///   "categoryId": "550e8400-e29b-41d4-a716-446655440001",
        ///   "categoryName": "Updated Electronics",
        ///   "createdAt": "2024-01-15T10:30:00Z",
        ///   "updatedAt": "2024-01-20T15:30:00Z"
        /// }
        /// </remarks>
        /// <param name="id">The unique identifier (GUID) of the base product to update.</param>
        /// <param name="request">The base product update request containing fields to modify.</param>
        /// <returns>The complete updated base product information.</returns>
        /// <response code="200">Base product updated successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="404">Base product not found with the specified ID</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateBaseProduct")]
        [ProducesResponseType(typeof(BaseProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateBaseProduct(Guid id, [FromBody] UpdateBaseProductRequest request)
        {
            _logger.LogInformation("Updating base product with ID: {BaseProductId}", id);

            var command = new UpdateBaseProductCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Base product updated successfully with ID: {BaseProductId}", result.Id);
                return this.ApiSuccess(result, "Base product updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Base product update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Base product update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating base product");
                return this.ApiError("An error occurred while updating the base product", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a base product from the system.
        /// </summary>
        /// <remarks>
        /// Requires Admin role authorization.
        /// This operation permanently removes the base product and cannot be undone.
        ///
        /// Sample request:
        /// DELETE /api/base-products/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (204 No Content): (empty body)
        /// </remarks>
        /// <param name="id">The unique identifier (GUID) of the base product to delete.</param>
        /// <returns>No content (successful deletion).</returns>
        /// <response code="204">Base product deleted successfully</response>
        /// <response code="404">Base product not found with the specified ID</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteBaseProduct")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBaseProduct(Guid id)
        {
            _logger.LogInformation("Deleting base product with ID: {BaseProductId}", id);

            var command = new DeleteBaseProductCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Base product deleted successfully with ID: {BaseProductId}", id);
                return this.ApiSuccess<object>("Base product deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Base product deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting base product");
                return this.ApiError("An error occurred while deleting the base product", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves a paginated list of base products with optional filtering.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        ///
        /// Supports filtering options:
        /// - Search term filtering (partial match, case-insensitive)
        /// - Category filtering (exact match)
        ///
        /// Sample request:
        /// GET /api/base-products?pageNumber=1&pageSize=10&searchTerm=laptop&categoryId=550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (200 OK):
        /// [
        ///   {
        ///     "id": "550e8400-e29b-41d4-a716-446655440000",
        ///     "name": "Laptop Computer",
        ///     "description": "High-performance laptop",
        ///     "categoryId": "550e8400-e29b-41d4-a716-446655440000",
        ///     "categoryName": "Electronics",
        ///     "createdAt": "2024-01-15T10:30:00Z",
        ///     "updatedAt": "2024-01-15T10:30:00Z"
        ///   }
        /// ]
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1). Must be greater than 0.</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100). Controls pagination size.</param>
        /// <param name="searchTerm">Optional search term for filtering base products by name or description.</param>
        /// <param name="categoryId">Optional category ID for filtering base products.</param>
        /// <returns>A list of base products matching the filter criteria.</returns>
        /// <response code="200">Base products list retrieved successfully</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        /// <response code="403">Forbidden - Admin or Viewer role required</response>
        [HttpGet]
        [Authorize(Policy = "CanViewBaseProduct")]
        [ProducesResponseType(typeof(List<BaseProductDto>), 200)]
        public async Task<IActionResult> GetBaseProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? categoryId = null)
        {
            _logger.LogInformation("Retrieving base products with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new GetBaseProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CategoryId = categoryId
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Base products retrieved successfully");
        }
    }
}