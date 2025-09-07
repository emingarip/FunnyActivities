using System;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a product variant is updated.
    /// </summary>
    public class ProductVariantUpdatedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the product variant that was updated.
        /// </summary>
        public ProductVariant ProductVariant { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantUpdatedEvent"/> class.
        /// </summary>
        /// <param name="productVariant">The product variant that was updated.</param>
        public ProductVariantUpdatedEvent(ProductVariant productVariant)
        {
            ProductVariant = productVariant;
            OccurredOn = DateTime.UtcNow;
        }
    }
}