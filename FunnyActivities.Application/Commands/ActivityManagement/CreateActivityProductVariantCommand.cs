using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for creating a new activity product variant.
    /// </summary>
    public class CreateActivityProductVariantCommand : IRequest<ActivityProductVariantDto>
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; set; }

        /// <summary>
        /// Gets or sets the quantity required for the activity.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        public Guid UnitOfMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user creating the activity product variant.
        /// </summary>
        public Guid UserId { get; set; }
    }
}