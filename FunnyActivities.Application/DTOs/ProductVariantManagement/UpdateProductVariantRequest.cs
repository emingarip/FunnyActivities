using Microsoft.AspNetCore.Http;

namespace FunnyActivities.Application.DTOs.ProductVariantManagement
{
    /// <summary>
    /// Request DTO for updating an existing product variant.
    /// </summary>
    public class UpdateProductVariantRequest
    {
        /// <summary>
        /// Gets or sets the name of the variant.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity.
        /// </summary>
        public decimal? StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        public Guid? UnitOfMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the unit value.
        /// </summary>
        public decimal? UnitValue { get; set; }

        /// <summary>
        /// Gets or sets the usage notes.
        /// </summary>
        public string? UsageNotes { get; set; }

        /// <summary>
        /// Gets or sets the list of photo files to upload.
        /// </summary>
        public List<IFormFile>? PhotoFiles { get; set; }

        /// <summary>
        /// Gets or sets the dynamic properties.
        /// </summary>
        public Dictionary<string, object>? DynamicProperties { get; set; }
    }
}