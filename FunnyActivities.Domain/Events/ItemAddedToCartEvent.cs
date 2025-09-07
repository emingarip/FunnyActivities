using System;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when an item is added to the shopping cart.
    /// </summary>
    public class ItemAddedToCartEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the shopping cart item that was added.
        /// </summary>
        public ShoppingCartItem ShoppingCartItem { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemAddedToCartEvent"/> class.
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item that was added.</param>
        public ItemAddedToCartEvent(ShoppingCartItem shoppingCartItem)
        {
            ShoppingCartItem = shoppingCartItem;
            OccurredOn = DateTime.UtcNow;
        }
    }
}