using System;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Data transfer object for activity category information.
    /// </summary>
    public class ActivityCategoryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the activity category.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the category.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the category was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the category was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the count of activities in this category.
        /// </summary>
        public int ActivityCount { get; set; }
    }
}