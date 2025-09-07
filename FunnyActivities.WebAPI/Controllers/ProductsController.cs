using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Application.Queries.BaseProductManagement;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.Queries.ProductVariantManagement;
using FunnyActivities.Application.DTOs.BaseProductManagement;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Application.DTOs.ProductsManagement;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.Domain.Exceptions;
using CreateBaseProductRequest = FunnyActivities.Application.DTOs.ProductsManagement.CreateBaseProductRequest;
using UpdateBaseProductRequest = FunnyActivities.Application.DTOs.ProductsManagement.UpdateBaseProductRequest;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Unified Products Controller for managing base products and their variants.
    /// Provides a single API surface for all product-related operations.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - Admin Role: Full CRUD operations
    /// - Viewer Role: Read-only operations
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/products")]
    public class ProductsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a paginated list of products with optional filtering.
        /// </summary>
        /// <remarks>
        /// Requires Admin or Viewer role authorization.
        /// Returns products with their variants in a unified format.
        /// </remarks>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="searchTerm">Optional search term for filtering products.</param>
        /// <param name="categoryId">Optional category ID for filtering products.</param>
        /// <param name="sortBy">Sort field (name, createdAt, updatedAt).</param>
        /// <param name="sortOrder">Sort order (asc, desc).</param>
        /// <returns>A paginated list of products with their variants.</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewBaseProduct")]
        [ProducesResponseType(typeof(PagedResult<ProductListDto>), 200)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc")
        {
            _logger.LogInformation("Retrieving products with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

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

            var baseProducts = await _mediator.Send(query);

            // For each base product, get its variants
            var productList = new List<ProductListDto>();
            foreach (var baseProduct in baseProducts)
            {
                var variantsQuery = new GetProductVariantsQuery
                {
                    BaseProductId = baseProduct.Id,
                    PageNumber = 1,
                    PageSize = 100 // Get all variants for this product
                };

                var variantsResult = await _mediator.Send(variantsQuery);

                var productListDto = new ProductListDto
                {
                    Id = baseProduct.Id,
                    Name = baseProduct.Name,
                    Description = baseProduct.Description,
                    CategoryId = baseProduct.CategoryId,
                    CategoryName = baseProduct.CategoryName,
                    CreatedAt = baseProduct.CreatedAt,
                    UpdatedAt = baseProduct.UpdatedAt,
                    Variants = variantsResult.Items.Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        BaseProductId = v.BaseProductId,
                        BaseProductName = v.BaseProductName,
                        Name = v.Name,
                        StockQuantity = v.StockQuantity,
                        UnitOfMeasureId = v.UnitOfMeasureId,
                        UnitOfMeasureName = v.UnitOfMeasureName,
                        UnitSymbol = v.UnitSymbol,
                        UnitValue = v.UnitValue,
                        UsageNotes = v.UsageNotes,
                        Photos = v.Photos,
                        DynamicProperties = v.DynamicProperties,
                        CreatedAt = v.CreatedAt,
                        UpdatedAt = v.UpdatedAt
                    }).ToList(),
                    TotalVariants = variantsResult.TotalCount,
                    BasePrice = null, // TODO: Calculate from variants if needed
                    StockStatus = variantsResult.Items.Any(v => v.StockQuantity > 0) ? "in-stock" : "out-of-stock"
                };

                productList.Add(productListDto);
            }

            // Calculate pagination info (simplified for now)
            var totalCount = productList.Count; // This is not accurate for pagination
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var result = new PagedResult<ProductListDto>(productList, pageNumber, pageSize, totalCount);

            return this.ApiSuccess(result, "Products retrieved successfully");
        }

        /// <summary>
        /// Retrieves a specific product by its unique identifier with all its variants.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>The product with all its variants.</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewBaseProduct")]
        [ProducesResponseType(typeof(ProductListDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

            // Get base product
            var baseProductQuery = new GetBaseProductQuery { Id = id };
            var baseProduct = await _mediator.Send(baseProductQuery);

            if (baseProduct == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return this.ApiError("Product not found", "NotFound", 404);
            }

            // Get variants
            var variantsQuery = new GetProductVariantsQuery
            {
                BaseProductId = id,
                PageNumber = 1,
                PageSize = 100
            };

            var variantsResult = await _mediator.Send(variantsQuery);

            var productDto = new ProductListDto
            {
                Id = baseProduct.Id,
                Name = baseProduct.Name,
                Description = baseProduct.Description,
                CategoryId = baseProduct.CategoryId,
                CategoryName = baseProduct.CategoryName,
                CreatedAt = baseProduct.CreatedAt,
                UpdatedAt = baseProduct.UpdatedAt,
                Variants = variantsResult.Items.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    BaseProductId = v.BaseProductId,
                    BaseProductName = v.BaseProductName,
                    Name = v.Name,
                    StockQuantity = v.StockQuantity,
                    UnitOfMeasureId = v.UnitOfMeasureId,
                    UnitOfMeasureName = v.UnitOfMeasureName,
                    UnitSymbol = v.UnitSymbol,
                    UnitValue = v.UnitValue,
                    UsageNotes = v.UsageNotes,
                    Photos = v.Photos,
                    DynamicProperties = v.DynamicProperties,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                }).ToList(),
                TotalVariants = variantsResult.TotalCount,
                BasePrice = null,
                StockStatus = variantsResult.Items.Any(v => v.StockQuantity > 0) ? "in-stock" : "out-of-stock"
            };

            return this.ApiSuccess(productDto, "Product retrieved successfully");
        }

        /// <summary>
        /// Creates a new base product.
        /// </summary>
        /// <param name="request">The base product creation request.</param>
        /// <returns>The created base product.</returns>
        [HttpPost("base")]
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
        /// Updates an existing base product.
        /// </summary>
        /// <param name="id">The unique identifier of the base product to update.</param>
        /// <param name="request">The base product update request.</param>
        /// <returns>The updated base product.</returns>
        [HttpPut("base/{id}")]
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
        /// Deletes a base product.
        /// </summary>
        /// <param name="id">The unique identifier of the base product to delete.</param>
        /// <param name="cascadeDeleteVariants">Whether to cascade delete associated variants. Defaults to false.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("base/{id}")]
        [Authorize(Policy = "CanDeleteBaseProduct")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeleteBaseProduct(Guid id, [FromQuery] bool cascadeDeleteVariants = false)
        {
            _logger.LogInformation("Deleting base product with ID: {BaseProductId}", id);
            _logger.LogInformation("Backend: Received cascadeDeleteVariants parameter: {CascadeDeleteVariants}", cascadeDeleteVariants);

            var command = new DeleteBaseProductCommand
            {
                Id = id,
                UserId = CurrentUserId,
                CascadeDeleteVariants = cascadeDeleteVariants
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Base product deleted successfully with ID: {BaseProductId}", id);
                return this.ApiSuccess<object>("Base product deleted successfully", 204);
            }
            catch (BaseProductNotFoundException ex)
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
        /// Retrieves all product variants across all base products.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <returns>A paginated list of all product variants.</returns>
        [HttpGet("variants")]
        [Authorize(Policy = "CanViewProductVariant")]
        [ProducesResponseType(typeof(PagedResult<ProductVariantDto>), 200)]
        public async Task<IActionResult> GetAllProductVariants(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Retrieving all product variants with page: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new GetProductVariantsQuery
            {
                BaseProductId = null, // null means get all variants
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "All product variants retrieved successfully");
        }

        /// <summary>
        /// Retrieves product variants for a specific base product.
        /// </summary>
        /// <param name="baseProductId">The unique identifier of the base product.</param>
        /// <param name="pageNumber">The page number (1-based, default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <returns>A paginated list of product variants.</returns>
        [HttpGet("{baseProductId}/variants")]
        [Authorize(Policy = "CanViewProductVariant")]
        [ProducesResponseType(typeof(PagedResult<ProductVariantDto>), 200)]
        public async Task<IActionResult> GetProductVariants(
            Guid baseProductId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Retrieving variants for base product ID: {BaseProductId}", baseProductId);

            // Validate pageSize
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var query = new GetProductVariantsQuery
            {
                BaseProductId = baseProductId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Product variants retrieved successfully");
        }

        /// <summary>
        /// Creates a new product variant.
        /// </summary>
        /// <param name="request">The product variant creation request.</param>
        /// <returns>The created product variant.</returns>
        [HttpPost("variants")]
        [Authorize(Policy = "CanCreateProductVariant")]
        [ProducesResponseType(typeof(ProductVariantDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateProductVariant([FromBody] CreateProductVariantRequest request)
        {
            _logger.LogInformation("Creating new product variant: {Name}", request.Name);

            var command = new CreateProductVariantCommand
            {
                BaseProductId = request.BaseProductId,
                Name = request.Name,
                StockQuantity = request.StockQuantity,
                UnitOfMeasureId = request.UnitOfMeasureId,
                UnitValue = request.UnitValue,
                UsageNotes = request.UsageNotes,
                PhotoFiles = request.PhotoFiles,
                DynamicProperties = request.DynamicProperties,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Product variant created successfully with ID: {Id}", result.Id);
                return this.ApiSuccess(result, "Product variant created successfully", 201);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Product variant creation failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating product variant");
                return this.ApiError("An error occurred while creating the product variant", "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates an existing product variant.
        /// </summary>
        /// <param name="id">The unique identifier of the product variant to update.</param>
        /// <param name="request">The product variant update request.</param>
        /// <returns>The updated product variant.</returns>
        [HttpPut("variants/{id}")]
        [Authorize(Policy = "CanUpdateProductVariant")]
        [ProducesResponseType(typeof(ProductVariantDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProductVariant(Guid id, [FromBody] UpdateProductVariantRequest request)
        {
            _logger.LogInformation("Updating product variant with ID: {ProductVariantId}", id);

            var command = new UpdateProductVariantCommand
            {
                Id = id,
                Name = request.Name,
                StockQuantity = request.StockQuantity,
                UnitOfMeasureId = request.UnitOfMeasureId,
                UnitValue = request.UnitValue,
                UsageNotes = request.UsageNotes,
                PhotoFiles = request.PhotoFiles,
                DynamicProperties = request.DynamicProperties,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Product variant updated successfully with ID: {ProductVariantId}", result.Id);
                return this.ApiSuccess(result, "Product variant updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product variant update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Product variant update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating product variant");
                return this.ApiError("An error occurred while updating the product variant", "InternalError", 500);
            }
        }

        /// <summary>
        /// Deletes a product variant.
        /// </summary>
        /// <param name="id">The unique identifier of the product variant to delete.</param>
        /// <returns>No content on successful deletion.</returns>
        [HttpDelete("variants/{id}")]
        [Authorize(Policy = "CanDeleteProductVariant")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProductVariant(Guid id)
        {
            _logger.LogInformation("Deleting product variant with ID: {ProductVariantId}", id);

            var command = new DeleteProductVariantCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Product variant deleted successfully with ID: {ProductVariantId}", id);
                return this.ApiSuccess<object>("Product variant deleted successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product variant deletion failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting product variant");
                return this.ApiError("An error occurred while deleting the product variant", "InternalError", 500);
            }
        }

        /// <summary>
        /// Performs bulk update operations on product variants.
        /// </summary>
        /// <param name="request">The bulk update request containing multiple variant updates.</param>
        /// <returns>The bulk update response with results and any errors.</returns>
        [HttpPut("variants/bulk")]
        [Authorize(Policy = "CanUpdateProductVariant")]
        [ProducesResponseType(typeof(BulkUpdateProductVariantsResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> BulkUpdateProductVariants([FromBody] BulkUpdateProductVariantsRequest request)
        {
            _logger.LogInformation("Performing bulk update on {Count} product variants", request.Updates?.Count ?? 0);

            var command = new BulkUpdateProductVariantsCommand
            {
                Updates = request.Updates?.Select(u => new FunnyActivities.Application.Commands.ProductVariantManagement.ProductVariantUpdateRequest
                {
                    Id = u.Id,
                    Name = u.Name,
                    StockQuantity = u.StockQuantity,
                    UnitOfMeasureId = u.UnitOfMeasureId,
                    UnitValue = u.UnitValue,
                    UsageNotes = u.UsageNotes,
                    DynamicProperties = u.DynamicProperties
                }).ToList() ?? new List<FunnyActivities.Application.Commands.ProductVariantManagement.ProductVariantUpdateRequest>(),
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Bulk update completed: {Successful}/{Total} variants updated",
                    result.SuccessfulUpdates, result.TotalUpdates);
                return this.ApiSuccess(result, "Bulk update completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during bulk update");
                return this.ApiError("An error occurred during bulk update", "InternalError", 500);
            }
        }

        // TODO: Add categories endpoints
        // TODO: Add drafts endpoints
    }
}