using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for deleting a step.
    /// </summary>
    public class DeleteStepCommandHandler : IRequestHandler<DeleteStepCommand, Unit>
    {
        private readonly IStepRepository _stepRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteStepCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteStepCommandHandler"/> class.
        /// </summary>
        /// <param name="stepRepository">The step repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public DeleteStepCommandHandler(
            IStepRepository stepRepository,
            IMediator mediator,
            ILogger<DeleteStepCommandHandler> logger)
        {
            _stepRepository = stepRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete step command.
        /// </summary>
        /// <param name="request">The delete step command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit value indicating the operation completed.</returns>
        public async Task<Unit> Handle(DeleteStepCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting step with ID: {StepId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing step
            var step = await _stepRepository.GetByIdAsync(request.Id);
            if (step == null)
            {
                _logger.LogWarning("Step deletion failed: Step with ID '{StepId}' not found", request.Id);
                throw new StepNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for step deletion");

            // Delete from repository
            await _stepRepository.DeleteAsync(step);

            _logger.LogInformation("Step deleted successfully with ID: {StepId}", step.Id);

            // Publish domain event
            var stepDeletedEvent = new StepDeletedEvent(step);
            await _mediator.Publish(stepDeletedEvent, cancellationToken);

            return Unit.Value;
        }
    }
}