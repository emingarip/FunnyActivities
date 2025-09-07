using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.ShoppingCartManagement;
using FunnyActivities.Application.DTOs.ShoppingCartManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ShoppingCartManagement
{
    /// <summary>
    /// Handler for adding an item to the shopping cart.
    /// </summary>
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, ShoppingCartItemDto>
    {
        private readonly IShoppingCartItemRepository _shoppingCartItemRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<AddToCartCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddToCartCommandHandler"/> class.
        /// </summary>
        /// <param name="shoppingCartItemRepository">The shopping cart item repository.</param>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public AddToCartCommandHandler(
            IShoppingCartItemRepository shoppingCartItemRepository,
            IProductVariantRepository productVariantRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<AddToCartCommandHandler> logger)
        {
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _productVariantRepository = productVariantRepository;
            _userRepository = userRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the add to cart command.
        /// </summary>
        /// <param name="request">The add to cart command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The shopping cart item DTO.</returns>
        public async Task<ShoppingCartItemDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding item to cart for user: {UserId}, product variant: {ProductVariantId}", request.UserId, request.ProductVariantId);

            // Check if product variant exists
            var productVariant = await _productVariantRepository.GetByIdAsync(request.ProductVariantId);
            if (productVariant == null)
            {
                _logger.LogWarning("Add to cart failed: Product variant with ID '{ProductVariantId}' not found", request.ProductVariantId);
                throw new ProductVariantNotFoundException(request.ProductVariantId);
            }

            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Add to cart failed: User with ID '{UserId}' not found", request.UserId);
                throw new Exception("User not found."); // TODO: Create UserNotFoundException
            }

            // Check if item already exists in cart
            var existingItem = await _shoppingCartItemRepository.GetByUserAndProductVariantAsync(request.UserId, request.ProductVariantId);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.UpdateQuantity(existingItem.Quantity + request.Quantity);
                await _shoppingCartItemRepository.UpdateAsync(existingItem);
                _logger.LogInformation("Updated existing cart item quantity");

                return new ShoppingCartItemDto
                {
                    Id = existingItem.Id,
                    ProductVariantId = existingItem.ProductVariantId,
                    ProductVariantName = existingItem.ProductVariant?.Name,
                    UserId = existingItem.UserId,
                    Quantity = existingItem.Quantity,
                    AddedAt = existingItem.AddedAt
                };
            }

            // Create new cart item
            var cartItem = ShoppingCartItem.Create(request.ProductVariantId, request.UserId, request.Quantity);
            await _shoppingCartItemRepository.AddAsync(cartItem);

            _logger.LogInformation("Item added to cart successfully with ID: {CartItemId}", cartItem.Id);

            // Publish domain event
            var itemAddedToCartEvent = new ItemAddedToCartEvent(cartItem);
            await _mediator.Publish(itemAddedToCartEvent, cancellationToken);

            // Return DTO
            return new ShoppingCartItemDto
            {
                Id = cartItem.Id,
                ProductVariantId = cartItem.ProductVariantId,
                ProductVariantName = cartItem.ProductVariant?.Name,
                UserId = cartItem.UserId,
                Quantity = cartItem.Quantity,
                AddedAt = cartItem.AddedAt
            };
        }
    }
}