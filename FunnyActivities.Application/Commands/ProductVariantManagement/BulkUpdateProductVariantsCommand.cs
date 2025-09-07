using MediatR;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using Microsoft.AspNetCore.Http;

namespace FunnyActivities.Application.Commands.ProductVariantManagement
{
    /// <summary>
    /// Command for bulk updating multiple product variants.
    /// </summary>
    public class BulkUpdateProductVariantsCommand : IRequest<BulkUpdateProductVariantsResponse>
    {
        /// <summary>
        /// Gets or sets the list of variant updates.
        /// </summary>
        public List<ProductVariantUpdateRequest> Updates { get; set; } = new();

        /// <summary>
        /// Gets or sets the ID of the user performing the bulk update.
        /// </summary>
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// Request model for individual variant update in bulk operation.
    /// </summary>
    public class ProductVariantUpdateRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product variant to update.
        /// </summary>
        public Guid Id { get; set; }

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

    /// <summary>
    /// Response model for bulk update operation.
    /// </summary>
    public class BulkUpdateProductVariantsResponse
    {
        /// <summary>
        /// Gets or sets the list of successfully updated variants.
        /// </summary>
        public List<ProductVariantDto> UpdatedVariants { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of failed updates with error details.
        /// </summary>
        public List<BulkUpdateError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of updates attempted.
        /// </summary>
        public int TotalUpdates { get; set; }

        /// <summary>
        /// Gets or sets the number of successful updates.
        /// </summary>
        public int SuccessfulUpdates { get; set; }

        /// <summary>
        /// Gets or sets the number of failed updates.
        /// </summary>
        public int FailedUpdates { get; set; }
    }

    /// <summary>
    /// Error details for failed bulk update operations.
    /// </summary>
    public class BulkUpdateError
    {
        /// <summary>
        /// Gets or sets the ID of the variant that failed to update.
        /// </summary>
        public Guid VariantId { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error type.
        /// </summary>
        public string ErrorType { get; set; } = string.Empty;
    }
}