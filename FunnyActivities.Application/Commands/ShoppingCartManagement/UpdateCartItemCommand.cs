using MediatR;
using FunnyActivities.Application.DTOs.ShoppingCartManagement;

namespace FunnyActivities.Application.Commands.ShoppingCartManagement
{
    /// <summary>
    /// Command for updating a cart item quantity.
    /// </summary>
    public class UpdateCartItemCommand : IRequest<ShoppingCartItemDto>
    {
        /// <summary>
        /// Gets or sets the cart item ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the new quantity.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public Guid UserId { get; set; }
    }
}