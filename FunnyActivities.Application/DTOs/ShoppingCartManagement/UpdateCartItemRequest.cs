using System;

namespace FunnyActivities.Application.DTOs.ShoppingCartManagement
{
    /// <summary>
    /// Request DTO for updating a cart item quantity.
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>
        /// Gets or sets the new quantity.
        /// </summary>
        public decimal Quantity { get; set; }
    }
}