using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for searching categories based on a search term.
    /// </summary>
    public class SearchCategoriesQueryHandler : IRequestHandler<SearchCategoriesQuery, PagedResult<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<SearchCategoriesQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCategoriesQueryHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="logger">The logger.</param>
        public SearchCategoriesQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<SearchCategoriesQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the search categories query.
        /// </summary>
        /// <param name="request">The search categories query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result of category DTOs matching the search term.</returns>
        public async Task<PagedResult<CategoryDto>> Handle(SearchCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching categories with term: '{SearchTerm}', page: {PageNumber}, size: {PageSize}",
                request.SearchTerm, request.PageNumber, request.PageSize);

            // Get all categories (in a real implementation, this should be done with proper search in the repository)
            var allCategories = await _categoryRepository.GetAllAsync();

            // Apply search filter
            var searchTerm = request.SearchTerm.ToLower();
            var filteredCategories = allCategories
                .Where(c => c.Name.ToLower().Contains(searchTerm) ||
                           (c.Description != null && c.Description.ToLower().Contains(searchTerm)))
                .OrderBy(c => c.Name)
                .AsQueryable();

            // Apply pagination
            var totalCount = filteredCategories.Count();
            var paginatedCategories = filteredCategories
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = paginatedCategories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductCount = c.BaseProducts != null ? c.BaseProducts.Count : 0
                })
                .ToList();

            _logger.LogInformation("Search completed: found {Count} categories out of {TotalCount} matching '{SearchTerm}'",
                items.Count, totalCount, request.SearchTerm);

            return new PagedResult<CategoryDto>(items, request.PageNumber, request.PageSize, totalCount);
        }
    }
}