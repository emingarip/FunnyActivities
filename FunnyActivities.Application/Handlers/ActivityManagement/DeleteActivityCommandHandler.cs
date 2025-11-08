using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.CrossCuttingConcerns.Caching;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for deleting an activity.
    /// </summary>
    public class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, Unit>
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteActivityCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteActivityCommandHandler"/> class.
        /// </summary>
        /// <param name="activityRepository">The activity repository.</param>
        /// <param name="cache">The cache service.</param>
        /// <param name="logger">The logger.</param>
        public DeleteActivityCommandHandler(IActivityRepository activityRepository, ICacheService cache, ILogger<DeleteActivityCommandHandler> logger)
        {
            _activityRepository = activityRepository;
            _cache = cache;
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

            // Invalidate public activities cache since deleted activity might have been public
            await InvalidatePublicActivitiesCacheAsync();

            _logger.LogInformation("Activity deleted successfully with ID: {ActivityId}", request.Id);

            return Unit.Value;
        }

        private async Task InvalidatePublicActivitiesCacheAsync()
        {
            try
            {
                // Clear all public activities cache keys (simplified approach)
                // In a production system, you might want to use a pattern-based invalidation
                // For now, we'll clear a few common cache keys
                var commonCacheKeys = new[]
                {
                    "public_activities_1_10_name_asc",
                    "public_activities_1_20_name_asc",
                    "public_activities_1_50_name_asc"
                };

                foreach (var key in commonCacheKeys)
                {
                    await _cache.RemoveAsync(key);
                }

                _logger.LogInformation("Invalidated public activities cache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating public activities cache");
            }
        }
    }
}