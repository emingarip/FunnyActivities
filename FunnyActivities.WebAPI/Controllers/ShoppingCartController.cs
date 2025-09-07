using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using FunnyActivities.Application.Commands.ShoppingCartManagement;
using FunnyActivities.Application.Queries.ShoppingCartManagement;
using FunnyActivities.Application.DTOs.ShoppingCartManagement;
using FunnyActivities.WebAPI.Controllers.Base;

namespace FunnyActivities.WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing shopping cart operations with role-based authorization.
    /// </summary>
    /// <remarks>
    /// Authorization Requirements:
    /// - All operations require authenticated user (automatically scoped to current user)
    /// - Users can only manage their own cart items
    /// - All endpoints require valid JWT token authentication
    /// </remarks>
    [ApiController]
    [Route("api/shopping-cart")]
    [Authorize]
    public class ShoppingCartController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ShoppingCartController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger.</param>
        public ShoppingCartController(IMediator mediator, ILogger<ShoppingCartController> logger)
            : base(logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Adds an item to the user's shopping cart.
        /// </summary>
        /// <remarks>
        /// If the item already exists in the cart, the quantity will be updated.
        ///
        /// Sample request:
        /// POST /api/shopping-cart
        /// {
        ///   "productVariantId": "550e8400-e29b-41d4-a716-446655440000",
        ///   "quantity": 2.0
        /// }
        ///
        /// Sample response (201 Created):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440001",
        ///   "productVariantId": "550e8400-e29b-41d4-a716-446655440000",
        ///   "productVariantName": "Laptop 16GB RAM",
        ///   "userId": "550e8400-e29b-41d4-a716-446655440002",
        ///   "quantity": 2.0,
        ///   "addedAt": "2024-01-15T10:30:00Z"
        /// }
        /// </remarks>
        /// <param name="request">The add to cart request containing product variant ID and quantity.</param>
        /// <returns>The cart item information.</returns>
        /// <response code="201">Item added to cart successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCartItemDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            _logger.LogInformation("Adding item to cart for user {UserId}", CurrentUserId);

            var command = new AddToCartCommand
            {
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Item added to cart successfully for user {UserId}", CurrentUserId);
                return this.ApiSuccess(result, "Item added to cart successfully", 201);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Add to cart failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding item to cart");
                return this.ApiError("An error occurred while adding item to cart", "InternalError", 500);
            }
        }

        /// <summary>
        /// Updates the quantity of an item in the user's shopping cart.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// PUT /api/shopping-cart/550e8400-e29b-41d4-a716-446655440000
        /// {
        ///   "quantity": 3.0
        /// }
        ///
        /// Sample response (200 OK):
        /// {
        ///   "id": "550e8400-e29b-41d4-a716-446655440000",
        ///   "productVariantId": "550e8400-e29b-41d4-a716-446655440001",
        ///   "productVariantName": "Laptop 16GB RAM",
        ///   "userId": "550e8400-e29b-41d4-a716-446655440002",
        ///   "quantity": 3.0,
        ///   "addedAt": "2024-01-15T10:30:00Z"
        /// }
        /// </remarks>
        /// <param name="id">The unique identifier of the cart item.</param>
        /// <param name="request">The update request containing the new quantity.</param>
        /// <returns>The updated cart item information.</returns>
        /// <response code="200">Cart item updated successfully</response>
        /// <response code="400">Invalid request data or validation errors</response>
        /// <response code="404">Cart item not found</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ShoppingCartItemDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
        {
            _logger.LogInformation("Updating cart item {CartItemId} for user {UserId}", id, CurrentUserId);

            var command = new UpdateCartItemCommand
            {
                Id = id,
                Quantity = request.Quantity,
                UserId = CurrentUserId
            };

            try
            {
                var result = await _mediator.Send(command);
                _logger.LogInformation("Cart item updated successfully for user {UserId}", CurrentUserId);
                return this.ApiSuccess(result, "Cart item updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Cart item update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Cart item update failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "ValidationError", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating cart item");
                return this.ApiError("An error occurred while updating cart item", "InternalError", 500);
            }
        }

        /// <summary>
        /// Removes an item from the user's shopping cart.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// DELETE /api/shopping-cart/550e8400-e29b-41d4-a716-446655440000
        ///
        /// Sample response (204 No Content): (empty body)
        /// </remarks>
        /// <param name="id">The unique identifier of the cart item to remove.</param>
        /// <returns>No content (successful removal).</returns>
        /// <response code="204">Item removed from cart successfully</response>
        /// <response code="404">Cart item not found</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveFromCart(Guid id)
        {
            _logger.LogInformation("Removing cart item {CartItemId} for user {UserId}", id, CurrentUserId);

            var command = new RemoveFromCartCommand
            {
                Id = id,
                UserId = CurrentUserId
            };

            try
            {
                await _mediator.Send(command);
                _logger.LogInformation("Cart item removed successfully for user {UserId}", CurrentUserId);
                return this.ApiSuccess<object>("Item removed from cart successfully", 204);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Cart item removal failed: {Message}", ex.Message);
                return this.ApiError(ex.Message, "NotFound", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing cart item");
                return this.ApiError("An error occurred while removing item from cart", "InternalError", 500);
            }
        }

        /// <summary>
        /// Retrieves all items in the user's shopping cart.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /api/shopping-cart
        ///
        /// Sample response (200 OK):
        /// [
        ///   {
        ///     "id": "550e8400-e29b-41d4-a716-446655440000",
        ///     "productVariantId": "550e8400-e29b-41d4-a716-446655440001",
        ///     "productVariantName": "Laptop 16GB RAM",
        ///     "userId": "550e8400-e29b-41d4-a716-446655440002",
        ///     "quantity": 2.0,
        ///     "addedAt": "2024-01-15T10:30:00Z"
        ///   }
        /// ]
        /// </remarks>
        /// <returns>A list of all items in the user's shopping cart.</returns>
        /// <response code="200">Shopping cart retrieved successfully</response>
        /// <response code="401">Unauthorized - valid JWT token required</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ShoppingCartItemDto>), 200)]
        public async Task<IActionResult> GetShoppingCart()
        {
            _logger.LogInformation("Retrieving shopping cart for user {UserId}", CurrentUserId);

            var query = new GetShoppingCartQuery
            {
                UserId = CurrentUserId
            };

            var result = await _mediator.Send(query);
            return this.ApiSuccess(result, "Shopping cart retrieved successfully");
        }
    }
}