using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a product variant name already exists.
    /// </summary>
    public class ProductVariantNameAlreadyExistsException : Exception
    {
        /// <summary>
        /// Gets the name of the product variant.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the base product ID.
        /// </summary>
        public Guid BaseProductId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantNameAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="name">The name of the product variant.</param>
        /// <param name="baseProductId">The base product ID.</param>
        public ProductVariantNameAlreadyExistsException(string name, Guid baseProductId)
            : base($"Product variant with name '{name}' already exists for base product '{baseProductId}'.")
        {
            Name = name;
            BaseProductId = baseProductId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariantNameAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="name">The name of the product variant.</param>
        /// <param name="baseProductId">The base product ID.</param>
        /// <param name="message">The error message.</param>
        public ProductVariantNameAlreadyExistsException(string name, Guid baseProductId, string message)
            : base(message)
        {
            Name = name;
            BaseProductId = baseProductId;
        }
    }
}