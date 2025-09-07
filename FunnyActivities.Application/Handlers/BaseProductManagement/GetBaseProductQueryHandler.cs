using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.BaseProductManagement;
using FunnyActivities.Application.DTOs.BaseProductManagement;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.BaseProductManagement
{
    /// <summary>
    /// Handler for retrieving a single base product by ID.
    /// </summary>
    public class GetBaseProductQueryHandler : IRequestHandler<GetBaseProductQuery, BaseProductDto>
    {
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly ILogger<GetBaseProductQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBaseProductQueryHandler"/> class.
        /// </summary>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="logger">The logger.</param>
        public GetBaseProductQueryHandler(
            IBaseProductRepository baseProductRepository,
            ILogger<GetBaseProductQueryHandler> logger)
        {
            _baseProductRepository = baseProductRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get base product query.
        /// </summary>
        /// <param name="request">The get base product query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The base product DTO.</returns>
        public async Task<BaseProductDto> Handle(GetBaseProductQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving base product with ID: {BaseProductId}", request.Id);

            var baseProduct = await _baseProductRepository.GetByIdAsync(request.Id);
            if (baseProduct == null)
            {
                _logger.LogWarning("Base product retrieval failed: Base product with ID {BaseProductId} not found", request.Id);
                throw new BaseProductNotFoundException(request.Id);
            }

            _logger.LogInformation("Base product retrieved successfully with ID: {BaseProductId}", baseProduct.Id);

            return new BaseProductDto
            {
                Id = baseProduct.Id,
                Name = baseProduct.Name,
                Description = baseProduct.Description,
                CategoryId = baseProduct.CategoryId,
                CategoryName = baseProduct.Category?.Name,
                CreatedAt = baseProduct.CreatedAt,
                UpdatedAt = baseProduct.UpdatedAt
            };
        }
    }
}