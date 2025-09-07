using System;
using MediatR;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for deleting an activity.
    /// </summary>
    public class DeleteActivityCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the ID of the activity to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the activity.
        /// </summary>
        public Guid UserId { get; set; }
    }
}