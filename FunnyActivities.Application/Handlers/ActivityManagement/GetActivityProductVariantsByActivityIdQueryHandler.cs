using System.Collections.Generic;
using System.Linq;
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
    /// Handler for retrieving all activity product variants for a specific activity.
    /// </summary>
    public class GetActivityProductVariantsByActivityIdQueryHandler : IRequestHandler<GetActivityProductVariantsByActivityIdQuery, List<ActivityProductVariantDto>>
    {
        private readonly IActivityProductVariantRepository _activityProductVariantRepository;
        private readonly ILogger<GetActivityProductVariantsByActivityIdQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivityProductVariantsByActivityIdQueryHandler"/> class.
        /// </summary>
        /// <param name="activityProductVariantRepository">The activity product variant repository.</param>
        /// <param name="logger">The logger.</param>
        public GetActivityProductVariantsByActivityIdQueryHandler(IActivityProductVariantRepository activityProductVariantRepository, ILogger<GetActivityProductVariantsByActivityIdQueryHandler> logger)
        {
            _activityProductVariantRepository = activityProductVariantRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get activity product variants by activity ID query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of activity product variant DTOs for the activity.</returns>
        public async Task<List<ActivityProductVariantDto>> Handle(GetActivityProductVariantsByActivityIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving activity product variants for activity ID: {ActivityId}", request.ActivityId);

            var variants = await _activityProductVariantRepository.GetByActivityIdAsync(request.ActivityId);

            var variantDtos = variants
                .OrderBy(v => v.CreatedAt)
                .Select(variant => new ActivityProductVariantDto
                {
                    Id = variant.Id,
                    ActivityId = variant.ActivityId,
                    ActivityName = "Unknown", // Temporarily disable navigation property loading
                    ProductVariantId = variant.ProductVariantId,
                    ProductVariantName = "Unknown", // Temporarily disable navigation property loading
                    BaseProductName = "Unknown", // Temporarily disable navigation property loading
                    Quantity = variant.Quantity,
                    UnitOfMeasureId = variant.UnitOfMeasureId,
                    UnitOfMeasureName = "Unknown", // Temporarily disable navigation property loading
                    UnitSymbol = "Unknown", // Temporarily disable navigation property loading
                    CreatedAt = variant.CreatedAt,
                    UpdatedAt = variant.UpdatedAt
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} activity product variants for activity ID: {ActivityId}", variantDtos.Count, request.ActivityId);

            return variantDtos;
        }
    }
}