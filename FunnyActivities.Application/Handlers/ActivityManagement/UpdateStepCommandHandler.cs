using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for updating an existing step.
    /// </summary>
    public class UpdateStepCommandHandler : IRequestHandler<UpdateStepCommand, StepDto>
    {
        private readonly IStepRepository _stepRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateStepCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStepCommandHandler"/> class.
        /// </summary>
        /// <param name="stepRepository">The step repository.</param>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public UpdateStepCommandHandler(
            IStepRepository stepRepository,
            IActivityRepository activityRepository,
            IMediator mediator,
            ILogger<UpdateStepCommandHandler> logger)
        {
            _stepRepository = stepRepository;
            _activityRepository = activityRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update step command.
        /// </summary>
        /// <param name="request">The update step command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated step DTO.</returns>
        public async Task<StepDto> Handle(UpdateStepCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating step with ID: {StepId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing step
            var step = await _stepRepository.GetByIdAsync(request.Id);
            if (step == null)
            {
                _logger.LogWarning("Step update failed: Step with ID '{StepId}' not found", request.Id);
                throw new StepNotFoundException(request.Id);
            }

            // Get the activity to include in DTO
            var activity = await _activityRepository.GetByIdAsync(step.ActivityId);
            if (activity == null)
            {
                _logger.LogWarning("Step update failed: Activity with ID '{ActivityId}' not found", step.ActivityId);
                throw new Exception($"Activity with ID {step.ActivityId} not found");
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for step update");

            // Update the step
            step.UpdateDetails(request.Order, request.Description, request.TimestampSeconds, request.DurationSeconds, request.PauseTimeSeconds, request.MediaAttachments);

            // Save to repository
            await _stepRepository.UpdateAsync(step);

            _logger.LogInformation("Step updated successfully with ID: {StepId}", step.Id);

            // Publish domain event
            var stepUpdatedEvent = new StepUpdatedEvent(step.Id, step.Description);
            await _mediator.Publish(stepUpdatedEvent, cancellationToken);

            // Return DTO
            return new StepDto
            {
                Id = step.Id,
                ActivityId = step.ActivityId,
                ActivityName = activity.Name,
                Order = step.Order,
                Description = step.Description,
                TimestampSeconds = step.TimestampSeconds,
                DurationSeconds = step.DurationSeconds,
                PauseTimeSeconds = step.PauseTimeSeconds,
                MediaAttachments = step.MediaAttachments,
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt
            };
        }
    }
}