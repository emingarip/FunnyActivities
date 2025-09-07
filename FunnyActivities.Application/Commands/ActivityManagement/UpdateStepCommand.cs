using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for updating an existing step.
    /// </summary>
    public class UpdateStepCommand : IRequest<StepDto>
    {
        /// <summary>
        /// Gets or sets the ID of the step to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the order of the step in the activity.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user updating the step.
        /// </summary>
        public Guid UserId { get; set; }
    }
}