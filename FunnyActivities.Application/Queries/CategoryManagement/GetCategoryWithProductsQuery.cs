using System;
using MediatR;
using FunnyActivities.Application.DTOs.CategoryManagement;

namespace FunnyActivities.Application.Queries.CategoryManagement
{
    /// <summary>
    /// Query for retrieving a category with all its associated products.
    /// </summary>
    public class GetCategoryWithProductsQuery : IRequest<CategoryWithProductsDto?>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the category.
        /// </summary>
        public Guid Id { get; set; }
    }
}