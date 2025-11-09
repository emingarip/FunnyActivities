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
    /// Handler for retrieving a single step by ID.
    /// </summary>
    public class GetStepQueryHandler : IRequestHandler<GetStepQuery, StepDto?>
    {
        private readonly IStepRepository _stepRepository;
        private readonly ILogger<GetStepQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetStepQueryHandler"/> class.
        /// </summary>
        /// <param name="stepRepository">The step repository.</param>
        /// <param name="logger">The logger.</param>
        public GetStepQueryHandler(IStepRepository stepRepository, ILogger<GetStepQueryHandler> logger)
        {
            _stepRepository = stepRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get step query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The step DTO or null if not found.</returns>
        public async Task<StepDto?> Handle(GetStepQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving step with ID: {StepId}", request.Id);

            var step = await _stepRepository.GetByIdAsync(request.Id);

            if (step == null)
            {
                _logger.LogWarning("Step with ID {StepId} not found", request.Id);
                return null;
            }

            var stepDto = new StepDto
            {
                Id = step.Id,
                ActivityId = step.ActivityId,
                ActivityName = step.Activity?.Name ?? "Unknown",
                Order = step.Order,
                Description = step.Description,
                TimestampSeconds = step.TimestampSeconds,
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt
            };

            _logger.LogInformation("Successfully retrieved step with ID: {StepId}", request.Id);

            return stepDto;
        }
    }
}
