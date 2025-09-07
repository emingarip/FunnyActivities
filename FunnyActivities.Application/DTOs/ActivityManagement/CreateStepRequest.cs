using System;
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
    }
}