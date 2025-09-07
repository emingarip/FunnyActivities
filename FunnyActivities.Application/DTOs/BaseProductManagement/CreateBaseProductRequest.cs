using System;

namespace FunnyActivities.Application.DTOs.BaseProductManagement
{
    /// <summary>
    /// Request DTO for creating a new base product.
    /// </summary>
    public class CreateBaseProductRequest
    {
        /// <summary>
        /// Gets or sets the name of the base product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the base product.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the base product.
        /// </summary>
        public Guid? CategoryId { get; set; }
    }
}