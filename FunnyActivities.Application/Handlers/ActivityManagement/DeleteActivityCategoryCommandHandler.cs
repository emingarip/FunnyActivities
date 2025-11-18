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
    /// Handler for deleting an activity category.
    /// </summary>
    public class DeleteActivityCategoryCommandHandler : IRequestHandler<DeleteActivityCategoryCommand, Unit>
    {
        private readonly IActivityCategoryRepository _activityCategoryRepository;
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteActivityCategoryCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteActivityCategoryCommandHandler"/> class.
        /// </summary>
        /// <param name="activityCategoryRepository">The activity category repository.</param>
        /// <param name="cache">The cache service.</param>
        /// <param name="logger">The logger.</param>
        public DeleteActivityCategoryCommandHandler(
            IActivityCategoryRepository activityCategoryRepository,
            ICacheService cache,
            ILogger<DeleteActivityCategoryCommandHandler> logger)
        {
            _activityCategoryRepository = activityCategoryRepository;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete activity category command.
        /// </summary>
        /// <param name="request">The delete activity category command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<Unit> Handle(DeleteActivityCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting activity category with ID: {CategoryId} by user: {UserId}", request.Id, request.UserId);

            // Check if the category exists
            var category = await _activityCategoryRepository.GetByIdAsync(request.Id).ConfigureAwait(false);
            if (category == null)
            {
                _logger.LogWarning("Activity category delete failed: Category with ID '{CategoryId}' not found", request.Id);
                throw new KeyNotFoundException($"The activity category with ID {request.Id} could not be found. Please verify the category exists and try again.");
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for activity category deletion");

            // Check if category has activities (you might want to prevent deletion if it has activities)
            // For now, we'll allow deletion even if it has activities - they will become uncategorized

            cancellationToken.ThrowIfCancellationRequested();

            // Delete the activity category
            await _activityCategoryRepository.DeleteAsync(category).ConfigureAwait(false);

            // Invalidate activity categories cache
            await _cache.RemoveAsync("activity_categories");

            _logger.LogInformation("Activity category deleted successfully with ID: {CategoryId}", request.Id);

            return Unit.Value;
        }
    }
}