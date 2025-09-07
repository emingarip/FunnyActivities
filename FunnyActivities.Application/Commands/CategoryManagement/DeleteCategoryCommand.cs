using System;
using MediatR;

namespace FunnyActivities.Application.Commands.CategoryManagement
{
    /// <summary>
    /// Command for deleting a category.
    /// </summary>
    public class DeleteCategoryCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the category.
        /// </summary>
        public Guid UserId { get; set; }
    }
}