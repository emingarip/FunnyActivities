using System;
using MediatR;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for deleting a step.
    /// </summary>
    public class DeleteStepCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the ID of the step to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the step.
        /// </summary>
        public Guid UserId { get; set; }
    }
}