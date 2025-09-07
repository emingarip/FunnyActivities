using MediatR;
using FunnyActivities.Application.DTOs.ShoppingCartManagement;

namespace FunnyActivities.Application.Commands.ShoppingCartManagement
{
    /// <summary>
    /// Command for adding an item to the shopping cart.
    /// </summary>
    public class AddToCartCommand : IRequest<ShoppingCartItemDto>
    {
        /// <summary>
        /// Gets or sets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets the quantity to add.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public Guid UserId { get; set; }
    }
}