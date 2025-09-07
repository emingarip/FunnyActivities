using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Category Controller for managing categories in the system.
    /// Provides comprehensive CRUD operations for category management.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations
    /// - Viewer Role: Read-only operations
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoryController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public CategoryController(IMediator mediator, ILogger<CategoryController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of categories with optional filtering.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        /// Returns categories with their product counts.
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="searchTerm">Optional search term for filtering categories by name.</param>
        /// <param name="sortBy">Sort field (name, createdAt, updatedAt).</param>
        /// <param name="sortOrder">Sort order (asc, desc).</param>
        /// <returns>A paginated list of categories.</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewCategory")]
        [ProducesResponseType(typeof(PagedResult<CategoryDto>), 200)]
        public async Task<IActionResult> GetCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            _logger.LogInformation("Retrieving categories with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new GetCategoriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Categories retrieved successfully");
        }

        /// <summary>
        /// Retrieves a specific category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>The category information.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewCategory")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            _logger.LogInformation("Retrieving category with ID: {CategoryId}", id);

            var query = new GetCategoryQuery { Id = id };
            var category = await _mediator.Send(query);

            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return this.ApiError("Category not found", "NotFound", 404);
            }

            return this.ApiSuccess(category, "Category retrieved successfully");
        }

        /// <summary>
        /// Retrieves a category with all its associated products.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>The category with its products.</returns>
        [HttpGet("{id}/with-products")]
        [Authorize(Policy = "CanViewCategory")]
        [ProducesResponseType(typeof(CategoryWithProductsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategoryWithProducts(Guid id)
        {
            _logger.LogInformation("Retrieving category with products for ID: {CategoryId}", id);

            var query = new GetCategoryWithProductsQuery { Id = id };
            var category = await _mediator.Send(query);

            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return this.ApiError("Category not found", "NotFound", 404);
            }

            return this.ApiSuccess(category, "Category with products retrieved successfully");
        }

        /// <summary>
        /// Searches for categories based on the provided search criteria.
        /// </summary>
        /// <param name="searchTerm">The search term to filter categories.</param>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <returns>A paginated list of matching categories.</returns>
        [HttpGet("search")]
        [Authorize(Policy = "CanViewCategory")]
        [ProducesResponseType(typeof(PagedResult<CategoryDto>), 200)]
        public async Task<IActionResult> SearchCategories(
            [FromQuery] string searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Searching categories with term: {SearchTerm}", searchTerm);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return this.ApiError("Search term is required", "ValidationError", 400);
            }

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new SearchCategoriesQuery
            {
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Categories search completed successfully");
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="request">The category creation request.</param>
        /// <returns>The created category.</returns>
        [HttpPost]
        [Authorize(Policy = "CanCreateCategory")]
        [ProducesResponseType(typeof(CategoryDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            _logger.LogInformation("Creating new category: {Name}", request.Name);

            var command = new CreateCategoryCommand
            {
                Name = request.Name,
                Description = request.Description,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Category created successfully with ID: {Id}", result.Id);
                return this.ApiSuccess(result, "Category created successfully", 201);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Category creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating category");
                return this.ApiError("An error occurred while creating the category", "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The unique identifier of the category to update.</param>
        /// <param name="request">The category update request.</param>
        /// <returns>The updated category.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanUpdateCategory")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            _logger.LogInformation("Updating category with ID: {CategoryId}", id);

            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Category updated successfully with ID: {CategoryId}", result.Id);
                return this.ApiSuccess(result, "Category updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Category update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Category update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating category");
                return this.ApiError("An error occurred while updating the category", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="id">The unique identifier of the category to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDeleteCategory")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            _logger.LogInformation("Deleting category with ID: {CategoryId}", id);

            var command = new DeleteCategoryCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Category deleted successfully with ID: {CategoryId}", id);
                return this.ApiSuccess<object>("Category deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Category deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting category");
                return this.ApiError("An error occurred while deleting the category", "InternalError", 500);
            }
        }
    }
}