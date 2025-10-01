using System;
using System.Collections.Generic;
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
        /// Gets or sets the timestamp in seconds for the step.
        /// </summary>
        public int? TimestampSeconds { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds for the step.
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the pause time in seconds for the step.
        /// </summary>
        public int? PauseTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the media attachments for the step.
        /// </summary>
        public List<string> MediaAttachments { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user updating the step.
        /// </summary>
        public Guid UserId { get; set; }
    }
}