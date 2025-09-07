using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for deleting an activity.
    /// </summary>
    public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, Unit>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ILogger<DeleteActivityCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteActivityCommandHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="logger">The logger.</param>
        public DeleteActivityCommandHandler(IActivityRepository activityRepository, ILogger<DeleteActivityCommandHandler> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete activity command.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<Unit> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting activity with ID: {ActivityId}", request.Id);

            var activity = await _activityRepository.GetByIdAsync(request.Id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found", request.Id);
                throw new KeyNotFoundException($"Activity with ID {request.Id} not found");
            }

            // Delete the activity
            await _activityRepository.DeleteAsync(activity);

            _logger.LogInformation("Activity deleted successfully with ID: {ActivityId}", request.Id);

            return Unit.Value;
        }
    }
}