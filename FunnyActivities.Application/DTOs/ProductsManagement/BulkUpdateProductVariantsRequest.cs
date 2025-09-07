using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ProductsManagement
{
    /// <summary>
    /// Request DTO for bulk updating product variants.
    /// </summary>
    public class BulkUpdateProductVariantsRequest
    {
        /// <summary>
        /// Gets or sets the list of product variant updates.
        /// </summary>
        [Required]
        public List<ProductVariantUpdateRequest> Updates { get; set; } = new List<ProductVariantUpdateRequest>();
    }

    /// <summary>
    /// Request DTO for updating a single product variant in bulk operations.
    /// </summary>
    public class ProductVariantUpdateRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product variant.
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the product variant.
        /// </summary>
        [StringLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity of the product variant.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        public Guid? UnitOfMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the unit value.
        /// </summary>
        [Range(0.01, double.MaxValue)]
        public decimal? UnitValue { get; set; }

        /// <summary>
        /// Gets or sets the usage notes.
        /// </summary>
        [StringLength(500)]
        public string? UsageNotes { get; set; }

        /// <summary>
        /// Gets or sets the dynamic properties.
        /// </summary>
        public Dictionary<string, object>? DynamicProperties { get; set; }
    }
}