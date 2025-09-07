using System;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Data transfer object for activity product variant information.
    /// </summary>
    public class ActivityProductVariantDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the activity product variant.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the activity name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// Gets or sets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets the product variant name.
        /// </summary>
        public string ProductVariantName { get; set; }

        /// <summary>
        /// Gets or sets the base product name.
        /// </summary>
        public string BaseProductName { get; set; }

        /// <summary>
        /// Gets or sets the quantity required for the activity.
        /// </summary>
        public decimal Quantity { get; set; }

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
        /// Gets or sets the date and time when the association was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the association was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}