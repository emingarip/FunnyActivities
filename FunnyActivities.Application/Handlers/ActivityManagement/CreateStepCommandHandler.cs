using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for creating a new step.
    /// </summary>
    public class CreateStepCommandHandler : IRequestHandler<CreateStepCommand, StepDto>
    {
        private readonly IStepRepository _stepRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateStepCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateStepCommandHandler"/> class.
        /// </summary>
        /// <param name="stepRepository">The step repository.</param>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public CreateStepCommandHandler(
            IStepRepository stepRepository,
            IActivityRepository activityRepository,
            IMediator mediator,
            ILogger<CreateStepCommandHandler> logger)
        {
            _stepRepository = stepRepository;
            _activityRepository = activityRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create step command.
        /// </summary>
        /// <param name="request">The create step command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created step DTO.</returns>
        public async Task<StepDto> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating step with description: {Description} for activity: {ActivityId} by user: {UserId}", request.Description, request.ActivityId, request.UserId);

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for step creation");

            // Check if activity exists
            var activity = await _activityRepository.GetByIdAsync(request.ActivityId);
            if (activity == null)
            {
                _logger.LogWarning("Step creation failed: Activity with ID '{ActivityId}' not found", request.ActivityId);
                throw new Exception($"Activity with ID {request.ActivityId} not found");
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Create the step
            var step = Step.Create(
                request.ActivityId,
                request.Order,
                request.Description,
                request.TimestampSeconds
            );

            // Save to repository
            await _stepRepository.AddAsync(step).ConfigureAwait(false);

            _logger.LogInformation("Step created successfully with ID: {StepId}", step.Id);

            // Publish domain event
            var stepCreatedEvent = new StepCreatedEvent(step.Id, step.Description);
            await _mediator.Publish(stepCreatedEvent, cancellationToken).ConfigureAwait(false);

            // Return DTO
            return new StepDto
            {
                Id = step.Id,
                ActivityId = step.ActivityId,
                ActivityName = activity.Name,
                Order = step.Order,
                Description = step.Description,
                TimestampSeconds = step.TimestampSeconds,
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt
            };
        }
    }
}
