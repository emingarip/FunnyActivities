using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.ActivityManagement;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for retrieving a single activity product variant by ID.
    /// </summary>
    public class GetActivityProductVariantQueryHandler : IRequestHandler<GetActivityProductVariantQuery, ActivityProductVariantDto?>
    {
        private readonly IActivityProductVariantRepository _activityProductVariantRepository;
        private readonly ILogger<GetActivityProductVariantQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivityProductVariantQueryHandler"/> class.
        /// </summary>
        /// <param name="activityProductVariantRepository">The activity product variant repository.</param>
        /// <param name="logger">The logger.</param>
        public GetActivityProductVariantQueryHandler(IActivityProductVariantRepository activityProductVariantRepository, ILogger<GetActivityProductVariantQueryHandler> logger)
        {
            _activityProductVariantRepository = activityProductVariantRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get activity product variant query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The activity product variant DTO or null if not found.</returns>
        public async Task<ActivityProductVariantDto?> Handle(GetActivityProductVariantQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving activity product variant with ID: {VariantId}", request.Id);

            var variant = await _activityProductVariantRepository.GetByIdAsync(request.Id);

            if (variant == null)
            {
                _logger.LogWarning("Activity product variant with ID {VariantId} not found", request.Id);
                return null;
            }

            var variantDto = new ActivityProductVariantDto
            {
                Id = variant.Id,
                ActivityId = variant.ActivityId,
                ActivityName = variant.Activity?.Name ?? "Unknown",
                ProductVariantId = variant.ProductVariantId,
                ProductVariantName = variant.ProductVariant?.Name ?? "Unknown",
                BaseProductName = variant.ProductVariant?.BaseProduct?.Name ?? "Unknown",
                Quantity = variant.Quantity,
                UnitOfMeasureId = variant.UnitOfMeasureId,
                UnitOfMeasureName = variant.UnitOfMeasure?.Name ?? "Unknown",
                UnitSymbol = variant.UnitOfMeasure?.Symbol ?? "",
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };

            _logger.LogInformation("Successfully retrieved activity product variant with ID: {VariantId}", request.Id);

            return variantDto;
        }
    }
}