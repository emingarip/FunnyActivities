using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ActivityManagement
{
    /// <summary>
    /// Request model for creating a new activity category.
    /// </summary>
    public class CreateActivityCategoryRequest
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the category.
        /// </summary>
        [StringLength(500, ErrorMessage = "Category description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}