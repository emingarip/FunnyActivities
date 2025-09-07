using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a specific variant of a base product.
    /// </summary>
    public class ProductVariant
    {
        /// <summary>
        /// Gets the unique identifier of the product variant.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the base product ID.
        /// </summary>
        public Guid BaseProductId { get; private set; }

        /// <summary>
        /// Gets the base product.
        /// </summary>
        public BaseProduct BaseProduct { get; private set; }

        /// <summary>
        /// Gets the name of the variant (e.g., "Red - Small").
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the current stock quantity of the variant.
        /// </summary>
        public decimal StockQuantity { get; private set; }

        /// <summary>
        /// Gets the unit of measure ID.
        /// </summary>
        public Guid UnitOfMeasureId { get; private set; }

        /// <summary>
        /// Gets the unit of measure.
        /// </summary>
        public UnitOfMeasure UnitOfMeasure { get; private set; }

        /// <summary>
        /// Gets the unit value of the variant.
        /// </summary>
        public decimal UnitValue { get; private set; }

        /// <summary>
        /// Gets the usage notes for the variant.
        /// </summary>
        public string? UsageNotes { get; private set; }

        /// <summary>
        /// Gets the list of photo URLs for the variant.
        /// </summary>
        public List<string> Photos { get; private set; }

        /// <summary>
        /// Gets the dynamic properties for the variant.
        /// </summary>
        public Dictionary<string, object> DynamicProperties { get; private set; }

        /// <summary>
        /// Gets the date and time when the variant was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the variant was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVariant"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="baseProductId">The base product ID.</param>
        /// <param name="name">The name of the variant.</param>
        /// <param name="stockQuantity">The stock quantity.</param>
        /// <param name="unitOfMeasureId">The unit of measure ID.</param>
        /// <param name="unitValue">The unit value.</param>
        /// <param name="usageNotes">The usage notes.</param>
        public ProductVariant(Guid id, Guid baseProductId, string name, decimal stockQuantity, Guid unitOfMeasureId, decimal unitValue, string? usageNotes)
        {
            Id = id;
            BaseProductId = baseProductId;
            Name = name;
            StockQuantity = stockQuantity;
            UnitOfMeasureId = unitOfMeasureId;
            UnitValue = unitValue;
            UsageNotes = usageNotes;
            Photos = new List<string>();
            DynamicProperties = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private ProductVariant() { }

        /// <summary>
        /// Creates a new product variant instance.
        /// </summary>
        /// <param name="baseProductId">The base product ID.</param>
        /// <param name="name">The name of the variant.</param>
        /// <param name="stockQuantity">The stock quantity.</param>
        /// <param name="unitOfMeasureId">The unit of measure ID.</param>
        /// <param name="unitValue">The unit value.</param>
        /// <param name="usageNotes">The usage notes.</param>
        /// <returns>A new product variant instance.</returns>
        public static ProductVariant Create(Guid baseProductId, string name, decimal stockQuantity, Guid unitOfMeasureId, decimal unitValue, string? usageNotes)
        {
            return new ProductVariant(Guid.NewGuid(), baseProductId, name, stockQuantity, unitOfMeasureId, unitValue, usageNotes);
        }

        /// <summary>
        /// Updates the stock quantity of the variant.
        /// </summary>
        /// <param name="newQuantity">The new stock quantity.</param>
        public void UpdateStock(decimal newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(newQuantity));

            StockQuantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the details of the variant.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="unitOfMeasureId">The new unit of measure ID.</param>
        /// <param name="unitValue">The new unit value.</param>
        /// <param name="usageNotes">The new usage notes.</param>
        public void UpdateDetails(string name, Guid unitOfMeasureId, decimal unitValue, string? usageNotes)
        {
            Name = name;
            UnitOfMeasureId = unitOfMeasureId;
            UnitValue = unitValue;
            UsageNotes = usageNotes;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the photos of the variant.
        /// </summary>
        /// <param name="photos">The list of photo URLs.</param>
        public void UpdatePhotos(List<string> photos)
        {
            Photos = photos ?? new List<string>();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the dynamic properties of the variant.
        /// </summary>
        /// <param name="dynamicProperties">The dynamic properties.</param>
        public void UpdateDynamicProperties(Dictionary<string, object> dynamicProperties)
        {
            DynamicProperties = dynamicProperties ?? new Dictionary<string, object>();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the stock quantity.
        /// </summary>
        /// <returns>True if the stock quantity is valid; otherwise, false.</returns>
        public bool ValidateStock()
        {
            return StockQuantity >= 0;
        }
    }
}