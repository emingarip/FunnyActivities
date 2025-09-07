using System;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a product variant is deleted.
    /// </summary>
    public class ProductVariantDeletedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the product variant that was deleted.
        /// </summary>
        public ProductVariant ProductVariant { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantDeletedEvent"/> class.
        /// </summary>
        /// <param name="productVariant">The product variant that was deleted.</param>
        public ProductVariantDeletedEvent(ProductVariant productVariant)
        {
            ProductVariant = productVariant;
            OccurredOn = DateTime.UtcNow;
        }
    }
}