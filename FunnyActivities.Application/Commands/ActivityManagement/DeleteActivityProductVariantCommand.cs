using System;
using MediatR;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for deleting an activity product variant.
    /// </summary>
    public class DeleteActivityProductVariantCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the ID of the activity product variant to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the activity product variant.
        /// </summary>
        public Guid UserId { get; set; }
    }
}