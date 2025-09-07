using System;
using MediatR;
using FunnyActivities.Application.DTOs.CategoryManagement;

namespace FunnyActivities.Application.Queries.CategoryManagement
{
    /// <summary>
    /// Query for retrieving a specific category by ID.
    /// </summary>
    public class GetCategoryQuery : IRequest<CategoryDto?>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category.
        /// </summary>
        public Guid Id { get; set; }
    }
}