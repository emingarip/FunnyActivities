using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents an item in a shopping cart.
    /// </summary>
    public class ShoppingCartItem
    {
        /// <summary>
        /// Gets the unique identifier of the cart item.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; private set; }

        /// <summary>
        /// Gets the product variant.
        /// </summary>
        public ProductVariant ProductVariant { get; private set; }

        /// <summary>
        /// Gets the user ID.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets the quantity of the item in the cart.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; private set; }

        /// <summary>
        /// Gets the date and time when the item was added to the cart.
        /// </summary>
        public DateTime AddedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="quantity">The quantity.</param>
        public ShoppingCartItem(Guid id, Guid productVariantId, Guid userId, decimal quantity)
        {
            Id = id;
            ProductVariantId = productVariantId;
            UserId = userId;
            Quantity = quantity;
            AddedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private ShoppingCartItem() { }

        /// <summary>
        /// Creates a new shopping cart item instance.
        /// </summary>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns>A new shopping cart item instance.</returns>
        public static ShoppingCartItem Create(Guid productVariantId, Guid userId, decimal quantity)
        {
            return new ShoppingCartItem(Guid.NewGuid(), productVariantId, userId, quantity);
        }

        /// <summary>
        /// Updates the quantity of the cart item.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        public void UpdateQuantity(decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            Quantity = quantity;
        }
    }
}