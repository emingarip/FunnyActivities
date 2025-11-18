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
using FunnyActivities.CrossCuttingConcerns.Caching;

namespace FunnyActivities.Application.Handlers.ActivityManagement
{
    /// <summary>
    /// Handler for updating an existing activity category.
    /// </summary>
    public class UpdateActivityCategoryCommandHandler : IRequestHandler<UpdateActivityCategoryCommand, ActivityCategoryDto>
    {
        private readonly IActivityCategoryRepository _activityCategoryRepository;
        private readonly ICacheService _cache;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateActivityCategoryCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivityCategoryCommandHandler"/> class.
        /// </summary>
        /// <param name="activityCategoryRepository">The activity category repository.</param>
        /// <param name="cache">The cache service.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public UpdateActivityCategoryCommandHandler(
            IActivityCategoryRepository activityCategoryRepository,
            ICacheService cache,
            IMediator mediator,
            ILogger<UpdateActivityCategoryCommandHandler> logger)
        {
            _activityCategoryRepository = activityCategoryRepository;
            _cache = cache;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update activity category command.
        /// </summary>
        /// <param name="request">The update activity category command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated activity category DTO.</returns>
        public async Task<ActivityCategoryDto> Handle(UpdateActivityCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating activity category with ID: {CategoryId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing category
            var category = await _activityCategoryRepository.GetByIdAsync(request.Id).ConfigureAwait(false);
            if (category == null)
            {
                _logger.LogWarning("Activity category update failed: Category with ID '{CategoryId}' not found", request.Id);
                throw new KeyNotFoundException($"The activity category with ID {request.Id} could not be found. Please verify the category exists and try again.");
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for activity category update");

            // Check for duplicate names (only if name is being changed)
            if (category.Name != request.Name)
            {
                var existingCategory = await _activityCategoryRepository.ExistsByNameAsync(request.Name).ConfigureAwait(false);
                if (existingCategory)
                {
                    _logger.LogWarning("Activity category update failed: Category with name '{Name}' already exists", request.Name);
                    throw new ActivityCategoryNameAlreadyExistsException(request.Name);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Update the activity category
            category.UpdateDetails(request.Name, request.Description);

            // Save to repository
            await _activityCategoryRepository.UpdateAsync(category).ConfigureAwait(false);

            // Invalidate activity categories cache
            await _cache.RemoveAsync("activity_categories");

            _logger.LogInformation("Activity category updated successfully with ID: {CategoryId}", category.Id);

            // Return DTO
            return new ActivityCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ActivityCount = category.Activities?.Count ?? 0
            };
        }
    }
}