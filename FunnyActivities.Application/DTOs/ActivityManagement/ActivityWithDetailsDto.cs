using System;
using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Data transfer object for activity with full details including steps and product variants.
    /// </summary>
    public class ActivityWithDetailsDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the activity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the video URL of the activity.
        /// </summary>
        public string? VideoUrl { get; set; }

        /// <summary>
        /// Gets or sets the duration of the activity in HH:MM:SS format.
        /// </summary>
        public string? Duration { get; set; }

        /// <summary>
        /// Gets or sets the activity category ID.
        /// </summary>
        public Guid ActivityCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the activity category name.
        /// </summary>
        public string ActivityCategoryName { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the activity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the activity was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the list of steps in the activity.
        /// </summary>
        public List<StepDto> Steps { get; set; } = new List<StepDto>();

        /// <summary>
        /// Gets or sets the list of activity product variants.
        /// </summary>
        public List<ActivityProductVariantDto> ActivityProductVariants { get; set; } = new List<ActivityProductVariantDto>();
    }
}