using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for retrieving a category with all its associated products.
    /// </summary>
    public class GetCategoryWithProductsQueryHandler : IRequestHandler<GetCategoryWithProductsQuery, CategoryWithProductsDto?>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoryWithProductsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetCategoryWithProductsQueryHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="logger">The logger.</param>
        public GetCategoryWithProductsQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoryWithProductsQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get category with products query.
        /// </summary>
        /// <param name="request">The get category with products query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The category with products DTO if found; otherwise, null.</returns>
        public async Task<CategoryWithProductsDto?> Handle(GetCategoryWithProductsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving category with products for ID: {CategoryId}", request.Id);

            var category = await _categoryRepository.GetByIdAsync(request.Id);

            if (category == null)
            {
                _logger.LogInformation("Category with ID '{CategoryId}' not found", request.Id);
                return null;
            }

            _logger.LogInformation("Category with products retrieved successfully with ID: {CategoryId}, products count: {ProductCount}",
                category.Id, category.BaseProducts?.Count ?? 0);

            return new CategoryWithProductsDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                Products = category.BaseProducts?.Select(bp => new BaseProductDto
                {
                    Id = bp.Id,
                    Name = bp.Name,
                    Description = bp.Description,
                    CategoryId = bp.CategoryId,
                    CategoryName = bp.Category?.Name,
                    CreatedAt = bp.CreatedAt,
                    UpdatedAt = bp.UpdatedAt
                }).ToList() ?? new List<BaseProductDto>(),
                TotalProducts = category.BaseProducts?.Count ?? 0
            };
        }
    }
}