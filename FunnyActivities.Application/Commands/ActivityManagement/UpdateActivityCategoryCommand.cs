using System;
using MediatR;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for updating an existing activity category.
    /// </summary>
    public class UpdateActivityCategoryCommand : IRequest<ActivityCategoryDto>
    {
        /// <summary>
        /// Gets or sets the ID of the category to update.
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