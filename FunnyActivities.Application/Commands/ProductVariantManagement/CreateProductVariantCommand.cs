using MediatR;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using Microsoft.AspNetCore.Http;

namespace FunnyActivities.Application.Commands.ProductVariantManagement
{
    /// <summary>
    /// Command for creating a new product variant.
    /// </summary>
    public class CreateProductVariantCommand : IRequest<ProductVariantDto>
    {
        /// <summary>
        /// Gets or sets the base product ID.
        /// </summary>
        public Guid BaseProductId { get; set; }

        /// <summary>
        /// Gets or sets the name of the variant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the initial stock quantity.
        /// </summary>
        public decimal StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        public Guid UnitOfMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the unit value.
        /// </summary>
        public decimal UnitValue { get; set; }

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

        /// <summary>
        /// Gets or sets the ID of the user creating the variant.
        /// </summary>
        public Guid UserId { get; set; }
    }
}