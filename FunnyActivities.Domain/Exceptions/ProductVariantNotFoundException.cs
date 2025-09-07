using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a product variant is not found.
    /// </summary>
    public class ProductVariantNotFoundException : Exception
    {
        /// <summary>
        /// Gets the ID of the product variant that was not found.
        /// </summary>
        public Guid ProductVariantId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantNotFoundException"/> class.
        /// </summary>
        /// <param name="productVariantId">The ID of the product variant that was not found.</param>
        public ProductVariantNotFoundException(Guid productVariantId)
            : base($"Product variant with ID '{productVariantId}' was not found.")
        {
            ProductVariantId = productVariantId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantNotFoundException"/> class.
        /// </summary>
        /// <param name="productVariantId">The ID of the product variant that was not found.</param>
        /// <param name="message">The error message.</param>
        public ProductVariantNotFoundException(Guid productVariantId, string message)
            : base(message)
        {
            ProductVariantId = productVariantId;
        }
    }
}