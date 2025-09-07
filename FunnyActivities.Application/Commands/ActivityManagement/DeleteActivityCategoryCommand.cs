using System;
using MediatR;

namespace FunnyActivities.Application.Commands.ActivityManagement
{
    /// <summary>
    /// Command for deleting an activity category.
    /// </summary>
    public class DeleteActivityCategoryCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the ID of the category to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the category.
        /// </summary>
        public Guid UserId { get; set; }
    }
}