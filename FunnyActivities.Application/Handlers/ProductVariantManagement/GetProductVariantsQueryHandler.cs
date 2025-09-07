using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.ProductVariantManagement;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Handlers.ProductVariantManagement
{
    /// <summary>
    /// Handler for getting all product variants.
    /// </summary>
    public class GetProductVariantsQueryHandler : IRequestHandler<GetProductVariantsQuery, PagedResult<ProductVariantDto>>
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ILogger<GetProductVariantsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetProductVariantsQueryHandler"/> class.
        /// </summary>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="logger">The logger.</param>
        public GetProductVariantsQueryHandler(
            IProductVariantRepository productVariantRepository,
            ILogger<GetProductVariantsQueryHandler> logger)
        {
            _productVariantRepository = productVariantRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get product variants query.
        /// </summary>
        /// <param name="request">The get product variants query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paged result of product variant DTOs.</returns>
        public async Task<PagedResult<ProductVariantDto>> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product variants with filters - BaseProductId: {BaseProductId}, SearchTerm: {SearchTerm}, UnitOfMeasureId: {UnitOfMeasureId}",
                request.BaseProductId, request.SearchTerm, request.UnitOfMeasureId);

            var productVariants = await _productVariantRepository.GetAllAsync();

            // Apply filters
            if (request.BaseProductId.HasValue)
            {
                productVariants = productVariants.Where(pv => pv.BaseProductId == request.BaseProductId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                productVariants = productVariants.Where(pv =>
                    pv.Name.ToLower().Contains(searchTerm) ||
                    (pv.BaseProduct?.Name != null && pv.BaseProduct.Name.ToLower().Contains(searchTerm)));
            }

            if (request.UnitOfMeasureId.HasValue)
            {
                productVariants = productVariants.Where(pv => pv.UnitOfMeasureId == request.UnitOfMeasureId.Value);
            }

            // Get total count before pagination
            var totalCount = productVariants.Count();

            // Apply pagination
            var paginatedVariants = productVariants
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            _logger.LogInformation("Retrieved {Count} product variants after filtering and pagination", paginatedVariants.Count);

            var dtos = paginatedVariants.Select(productVariant => new ProductVariantDto
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
            }).ToList();

            return new PagedResult<ProductVariantDto>(dtos, request.PageNumber, request.PageSize, totalCount);
        }
    }
}