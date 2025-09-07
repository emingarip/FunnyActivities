using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.BaseProductManagement;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.Handlers.BaseProductManagement
{
    /// <summary>
    /// Handler for retrieving a list of base products with optional filtering and pagination.
    /// </summary>
    public class GetBaseProductsQueryHandler : IRequestHandler<GetBaseProductsQuery, List<BaseProductDto>>
    {
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly ILogger<GetBaseProductsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBaseProductsQueryHandler"/> class.
        /// </summary>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="logger">The logger.</param>
        public GetBaseProductsQueryHandler(
            IBaseProductRepository baseProductRepository,
            ILogger<GetBaseProductsQueryHandler> logger)
        {
            _baseProductRepository = baseProductRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get base products query.
        /// </summary>
        /// <param name="request">The get base products query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of base product DTOs.</returns>
        public async Task<List<BaseProductDto>> Handle(GetBaseProductsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving base products with search term: {SearchTerm}, category ID: {CategoryId}, page: {PageNumber}, size: {PageSize}",
                request.SearchTerm, request.CategoryId, request.PageNumber, request.PageSize);

            var baseProducts = await _baseProductRepository.GetPagedAsync(
                request.SearchTerm,
                request.CategoryId,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} base products", baseProducts.Count);

            return baseProducts.Select(bp => new BaseProductDto
            {
                Id = bp.Id,
                Name = bp.Name,
                Description = bp.Description,
                CategoryId = bp.CategoryId,
                CategoryName = bp.Category?.Name,
                CreatedAt = bp.CreatedAt,
                UpdatedAt = bp.UpdatedAt
            }).ToList();
        }
    }
}