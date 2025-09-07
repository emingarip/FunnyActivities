using System;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Data transfer object for step information.
    /// </summary>
    public class StepDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the step.
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
        /// Gets or sets the order of the step in the activity.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the step was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the step was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}