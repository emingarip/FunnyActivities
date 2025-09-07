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
    /// Handler for creating a new activity category.
    /// </summary>
    public class CreateActivityCategoryCommandHandler : IRequestHandler<CreateActivityCategoryCommand, ActivityCategoryDto>
    {
        private readonly IActivityCategoryRepository _activityCategoryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateActivityCategoryCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateActivityCategoryCommandHandler"/> class.
        /// </summary>
        /// <param name="activityCategoryRepository">The activity category repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public CreateActivityCategoryCommandHandler(
            IActivityCategoryRepository activityCategoryRepository,
            IMediator mediator,
            ILogger<CreateActivityCategoryCommandHandler> logger)
        {
            _activityCategoryRepository = activityCategoryRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create activity category command.
        /// </summary>
        /// <param name="request">The create activity category command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created activity category DTO.</returns>
        public async Task<ActivityCategoryDto> Handle(CreateActivityCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating activity category with name: {Name} by user: {UserId}", request.Name, request.UserId);

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for activity category creation");

            // Check for duplicate names
            var existingCategory = await _activityCategoryRepository.ExistsByNameAsync(request.Name).ConfigureAwait(false);
            if (existingCategory)
            {
                _logger.LogWarning("Activity category creation failed: Category with name '{Name}' already exists", request.Name);
                throw new ActivityCategoryNameAlreadyExistsException(request.Name);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Create the activity category
            var category = ActivityCategory.Create(request.Name, request.Description);

            // Save to repository
            await _activityCategoryRepository.AddAsync(category).ConfigureAwait(false);

            _logger.LogInformation("Activity category created successfully with ID: {CategoryId}", category.Id);

            // Publish domain event
            var categoryCreatedEvent = new ActivityCategoryCreatedEvent(category);
            await _mediator.Publish(categoryCreatedEvent, cancellationToken).ConfigureAwait(false);

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