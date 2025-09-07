using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Request model for updating an existing activity.
    /// </summary>
    public class UpdateActivityRequest
    {
        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Activity name must be between 1 and 200 characters.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Activity description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the video URL of the activity.
        /// </summary>
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in hours.
        /// </summary>
        [Range(0, 23, ErrorMessage = "Hours must be between 0 and 23.")]
        public int? DurationHours { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in minutes.
        /// </summary>
        [Range(0, 59, ErrorMessage = "Minutes must be between 0 and 59.")]
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in seconds.
        /// </summary>
        [Range(0, 59, ErrorMessage = "Seconds must be between 0 and 59.")]
        public int? DurationSeconds { get; set; }
    }
}