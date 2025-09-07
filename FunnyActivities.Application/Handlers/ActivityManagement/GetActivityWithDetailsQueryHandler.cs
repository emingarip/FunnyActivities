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
    /// Handler for retrieving a single activity with full details including steps and product variants.
    /// </summary>
    public class GetActivityWithDetailsQueryHandler : IRequestHandler<GetActivityWithDetailsQuery, ActivityWithDetailsDto?>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<GetActivityWithDetailsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivityWithDetailsQueryHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public GetActivityWithDetailsQueryHandler(IActivityRepository activityRepository, ILogger<GetActivityWithDetailsQueryHandler> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get activity with details query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The activity with details DTO or null if not found.</returns>
        public async Task<ActivityWithDetailsDto?> Handle(GetActivityWithDetailsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving activity with details for ID: {ActivityId}", request.Id);

            var activity = await _activityRepository.GetByIdAsync(request.Id);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", request.Id);
                return null;
            }

            var activityDto = new ActivityWithDetailsDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                VideoUrl = activity.VideoUrl?.Value,
                Duration = activity.Duration?.ToString(),
                ActivityCategoryId = activity.ActivityCategoryId,
                ActivityCategoryName = activity.ActivityCategory?.Name ?? "Unknown",
                CreatedAt = activity.CreatedAt,
                UpdatedAt = activity.UpdatedAt,
                Steps = activity.Steps?.Select(s => new StepDto
                {
                    Id = s.Id,
                    ActivityId = s.ActivityId,
                    ActivityName = activity.Name,
                    Order = s.Order,
                    Description = s.Description,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).OrderBy(s => s.Order).ToList() ?? new List<StepDto>(),
                ActivityProductVariants = activity.ActivityProductVariants?.Select(apv => new ActivityProductVariantDto
                {
                    Id = apv.Id,
                    ActivityId = apv.ActivityId,
                    ActivityName = activity.Name,
                    ProductVariantId = apv.ProductVariantId,
                    ProductVariantName = apv.ProductVariant?.Name ?? "Unknown",
                    BaseProductName = apv.ProductVariant?.BaseProduct?.Name ?? "Unknown",
                    Quantity = apv.Quantity,
                    UnitOfMeasureId = apv.UnitOfMeasureId,
                    UnitOfMeasureName = apv.UnitOfMeasure?.Name ?? "Unknown",
                    UnitSymbol = apv.UnitOfMeasure?.Symbol ?? "Unknown",
                    CreatedAt = apv.CreatedAt,
                    UpdatedAt = apv.UpdatedAt
                }).ToList() ?? new List<ActivityProductVariantDto>()
            };

            _logger.LogInformation("Successfully retrieved activity with details for ID: {ActivityId}, Steps: {StepCount}, ProductVariants: {ProductVariantCount}",
                request.Id, activityDto.Steps.Count, activityDto.ActivityProductVariants.Count);

            return activityDto;
        }
    }
}