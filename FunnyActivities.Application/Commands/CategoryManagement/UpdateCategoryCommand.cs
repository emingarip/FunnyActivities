using System;
using MediatR;
using FunnyActivities.Application.DTOs.CategoryManagement;

namespace FunnyActivities.Application.Commands.CategoryManagement
{
    /// <summary>
    /// Command for updating an existing category.
    /// </summary>
    public class UpdateCategoryCommand : IRequest<CategoryDto>
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
        /// Gets or sets the ID of the user updating the category.
        /// </summary>
        public Guid UserId { get; set; }
    }
}