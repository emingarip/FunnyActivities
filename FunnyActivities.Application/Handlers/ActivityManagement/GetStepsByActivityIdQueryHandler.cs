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
    /// Handler for retrieving all steps for a specific activity.
    /// </summary>
    public class GetStepsByActivityIdQueryHandler : IRequestHandler<GetStepsByActivityIdQuery, List<StepDto>>
    {
        private readonly IStepRepository _stepRepository;
        private readonly ILogger<GetStepsByActivityIdQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetStepsByActivityIdQueryHandler"/> class.
        /// </summary>
        /// <param name="stepRepository">The step repository.</param>
        /// <param name="logger">The logger.</param>
        public GetStepsByActivityIdQueryHandler(IStepRepository stepRepository, ILogger<GetStepsByActivityIdQueryHandler> logger)
        {
            _stepRepository = stepRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the get steps by activity ID query.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of step DTOs for the activity.</returns>
        public async Task<List<StepDto>> Handle(GetStepsByActivityIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving steps for activity ID: {ActivityId}", request.ActivityId);

            var steps = await _stepRepository.GetByActivityIdAsync(request.ActivityId);

            var stepDtos = steps
                .OrderBy(s => s.Order)
                .Select(step => new StepDto
                {
                    Id = step.Id,
                    ActivityId = step.ActivityId,
                    ActivityName = "Unknown", // Temporarily disable navigation property loading
                    Order = step.Order,
                    Description = step.Description,
                    TimestampSeconds = step.TimestampSeconds,
                    CreatedAt = step.CreatedAt,
                    UpdatedAt = step.UpdatedAt
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} steps for activity ID: {ActivityId}", stepDtos.Count, request.ActivityId);

            return stepDtos;
        }
    }
}
