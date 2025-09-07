using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for updating an existing activity product variant.
    /// </summary>
    public class UpdateActivityProductVariantCommand : IRequest<ActivityProductVariantDto>
    {
        /// <summary>
        /// Gets or sets the ID of the activity product variant to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the quantity required for the activity.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID.
        /// </summary>
        public Guid UnitOfMeasureId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user updating the activity product variant.
        /// </summary>
        public Guid UserId { get; set; }
    }
}