using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.DTOs.CategoryManagement;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for retrieving a specific category by ID.
    /// </summary>
    public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, CategoryDto?>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoryQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetCategoryQueryHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="logger">The logger.</param>
        public GetCategoryQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoryQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get category query.
        /// </summary>
        /// <param name="request">The get category query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The category DTO if found; otherwise, null.</returns>
        public async Task<CategoryDto?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving category with ID: {CategoryId}", request.Id);

            var category = await _categoryRepository.GetByIdAsync(request.Id);

            if (category == null)
            {
                _logger.LogInformation("Category with ID '{CategoryId}' not found", request.Id);
                return null;
            }

            _logger.LogInformation("Category retrieved successfully with ID: {CategoryId}", category.Id);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = category.BaseProducts?.Count ?? 0
            };
        }
    }
}