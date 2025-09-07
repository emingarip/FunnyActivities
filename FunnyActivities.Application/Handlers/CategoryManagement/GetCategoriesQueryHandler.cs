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
    /// Handler for retrieving a paginated list of categories.
    /// </summary>
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoriesQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetCategoriesQueryHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="logger">The logger.</param>
        public GetCategoriesQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoriesQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get categories query.
        /// </summary>
        /// <param name="request">The get categories query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result of category DTOs.</returns>
        public async Task<PagedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving categories with page: {PageNumber}, size: {PageSize}, search: {SearchTerm}, sort: {SortBy} {SortOrder}",
                request.PageNumber, request.PageSize, request.SearchTerm, request.SortBy, request.SortOrder);

            // Get all categories (in a real implementation, this should be done with proper pagination in the repository)
            var allCategories = await _categoryRepository.GetAllAsync();

            // Apply search filter if provided
            var filteredCategories = allCategories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                filteredCategories = filteredCategories.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            var sortBy = request.SortBy?.ToLower() ?? "name";
            var sortOrder = request.SortOrder?.ToLower() ?? "asc";

            filteredCategories = sortBy switch
            {
                "name" => sortOrder == "desc"
                    ? filteredCategories.OrderByDescending(c => c.Name)
                    : filteredCategories.OrderBy(c => c.Name),
                "createdat" => sortOrder == "desc"
                    ? filteredCategories.OrderByDescending(c => c.CreatedAt)
                    : filteredCategories.OrderBy(c => c.CreatedAt),
                _ => sortOrder == "desc"
                    ? filteredCategories.OrderByDescending(c => c.Name)
                    : filteredCategories.OrderBy(c => c.Name)
            };

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

            _logger.LogInformation("Retrieved {Count} categories out of {TotalCount}", items.Count, totalCount);

            return new PagedResult<CategoryDto>(items, request.PageNumber, request.PageSize, totalCount);
        }
    }
}