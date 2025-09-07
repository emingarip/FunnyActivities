using System;

namespace FunnyActivities.Application.DTOs.ShoppingCartManagement
{
    /// <summary>
    /// Data transfer object for shopping cart item information.
    /// </summary>
    public class ShoppingCartItemDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the cart item.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets the product variant name.
        /// </summary>
        public string ProductVariantName { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the item in the cart.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the item was added to the cart.
        /// </summary>
        public DateTime AddedAt { get; set; }
    }
}