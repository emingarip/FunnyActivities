using System;

namespace FunnyActivities.Application.DTOs.ShoppingCartManagement
{
    /// <summary>
    /// Request DTO for adding an item to the shopping cart.
    /// </summary>
    public class AddToCartRequest
    {
        /// <summary>
        /// Gets or sets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets the quantity to add.
        /// </summary>
        public decimal Quantity { get; set; }
    }
}