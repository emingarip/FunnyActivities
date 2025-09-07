using System;
using System.Collections.Generic;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.DTOs.CategoryManagement
{
    /// <summary>
    /// Data transfer object for category information including related products.
    /// </summary>
    public class CategoryWithProductsDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category.
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
        /// Gets or sets the list of products in this category.
        /// </summary>
        public List<BaseProductDto> Products { get; set; } = new List<BaseProductDto>();

        /// <summary>
        /// Gets or sets the total count of products in this category.
        /// </summary>
        public int TotalProducts { get; set; }
    }
}