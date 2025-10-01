using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Request model for creating a new step.
    /// </summary>
    public class CreateStepRequest
    {
        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        [Required(ErrorMessage = "Activity ID is required.")]
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the order of the step in the activity.
        /// </summary>
        [Required(ErrorMessage = "Order is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than 0.")]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 1000 characters.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the timestamp in seconds for the step.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Timestamp seconds must be non-negative.")]
        public int? TimestampSeconds { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds for the step.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Duration seconds must be non-negative.")]
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the pause time in seconds for the step.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Pause time seconds must be non-negative.")]
        public int? PauseTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the media attachments for the step.
        /// </summary>
        public List<string> MediaAttachments { get; set; } = new List<string>();
    }
}