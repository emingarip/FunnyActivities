using System;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a product variant is created.
    /// </summary>
    public class ProductVariantCreatedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the product variant that was created.
        /// </summary>
        public ProductVariant ProductVariant { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantCreatedEvent"/> class.
        /// </summary>
        /// <param name="productVariant">The product variant that was created.</param>
        public ProductVariantCreatedEvent(ProductVariant productVariant)
        {
            ProductVariant = productVariant;
            OccurredOn = DateTime.UtcNow;
        }
    }
}