using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ProductsManagement
{
    /// <summary>
    /// Request DTO for creating a new base product.
    /// </summary>
    public class CreateBaseProductRequest
    {
        /// <summary>
        /// Gets or sets the name of the base product.
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the base product.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the base product.
        /// </summary>
        public Guid? CategoryId { get; set; }
    }
}