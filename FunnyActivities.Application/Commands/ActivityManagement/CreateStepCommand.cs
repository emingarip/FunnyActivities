using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for creating a new step.
    /// </summary>
    public class CreateStepCommand : IRequest<StepDto>
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the order of the step in the activity.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user creating the step.
        /// </summary>
        public Guid UserId { get; set; }
    }
}