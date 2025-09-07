using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Request model for updating an existing activity product variant.
    /// </summary>
    public class UpdateActivityProductVariantRequest
    {
        /// <summary>
        /// Gets or sets the quantity required for the activity.
        /// </summary>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        [Required(ErrorMessage = "Unit of measure ID is required.")]
        public Guid UnitOfMeasureId { get; set; }
    }
}