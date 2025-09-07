using System;

namespace FunnyActivities.Application.DTOs.ProductVariantManagement
{
    /// <summary>
    /// Data transfer object for product variant information.
    /// </summary>
    public class ProductVariantDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product variant.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the base product ID.
        /// </summary>
        public Guid BaseProductId { get; set; }

        /// <summary>
        /// Gets or sets the base product name.
        /// </summary>
        public string BaseProductName { get; set; }

        /// <summary>
        /// Gets or sets the base product description.
        /// </summary>
        public string? BaseProductDescription { get; set; }

        /// <summary>
        /// Gets or sets the base product category ID.
        /// </summary>
        public Guid? BaseProductCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the base product category name.
        /// </summary>
        public string? BaseProductCategoryName { get; set; }

        /// <summary>
        /// Gets or sets the name of the variant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current stock quantity of the variant.
        /// </summary>
        public decimal StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        public Guid UnitOfMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure name.
        /// </summary>
        public string UnitOfMeasureName { get; set; }

        /// <summary>
        /// Gets or sets the unit symbol.
        /// </summary>
        public string UnitSymbol { get; set; }

        /// <summary>
        /// Gets or sets the unit value of the variant.
        /// </summary>
        public decimal UnitValue { get; set; }

        /// <summary>
        /// Gets or sets the usage notes for the variant.
        /// </summary>
        public string? UsageNotes { get; set; }

        /// <summary>
        /// Gets or sets the list of photo URLs for the variant.
        /// </summary>
        public List<string> Photos { get; set; }

        /// <summary>
        /// Gets or sets the dynamic properties for the variant.
        /// </summary>
        public Dictionary<string, object> DynamicProperties { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the variant was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the variant was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}