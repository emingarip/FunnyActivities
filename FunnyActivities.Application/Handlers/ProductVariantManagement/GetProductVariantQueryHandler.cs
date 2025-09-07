using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.ProductVariantManagement;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ProductVariantManagement
{
    /// <summary>
    /// Handler for getting a product variant by ID.
    /// </summary>
    public class GetProductVariantQueryHandler : IRequestHandler<GetProductVariantQuery, ProductVariantDto>
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ILogger<GetProductVariantQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetProductVariantQueryHandler"/> class.
        /// </summary>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="logger">The logger.</param>
        public GetProductVariantQueryHandler(
            IProductVariantRepository productVariantRepository,
            ILogger<GetProductVariantQueryHandler> logger)
        {
            _productVariantRepository = productVariantRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get product variant query.
        /// </summary>
        /// <param name="request">The get product variant query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The product variant DTO.</returns>
        public async Task<ProductVariantDto> Handle(GetProductVariantQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product variant with ID: {ProductVariantId}", request.Id);

            var productVariant = await _productVariantRepository.GetByIdAsync(request.Id);
            if (productVariant == null)
            {
                _logger.LogWarning("Product variant with ID '{ProductVariantId}' not found", request.Id);
                throw new ProductVariantNotFoundException(request.Id);
            }

            _logger.LogInformation("Product variant retrieved successfully with ID: {ProductVariantId}", productVariant.Id);

            return new ProductVariantDto
            {
                Id = productVariant.Id,
                BaseProductId = productVariant.BaseProductId,
                BaseProductName = productVariant.BaseProduct?.Name,
                BaseProductDescription = productVariant.BaseProduct?.Description,
                BaseProductCategoryId = productVariant.BaseProduct?.CategoryId,
                BaseProductCategoryName = productVariant.BaseProduct?.Category?.Name,
                Name = productVariant.Name,
                StockQuantity = productVariant.StockQuantity,
                UnitOfMeasureId = productVariant.UnitOfMeasureId,
                UnitOfMeasureName = productVariant.UnitOfMeasure?.Name,
                UnitSymbol = productVariant.UnitOfMeasure?.Symbol,
                UnitValue = productVariant.UnitValue,
                UsageNotes = productVariant.UsageNotes,
                Photos = productVariant.Photos,
                DynamicProperties = productVariant.DynamicProperties,
                CreatedAt = productVariant.CreatedAt,
                UpdatedAt = productVariant.UpdatedAt
            };
        }
    }
}