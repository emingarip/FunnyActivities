using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for creating a new category.
    /// </summary>
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCategoryCommandHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IMediator mediator,
            ILogger<CreateCategoryCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create category command.
        /// </summary>
        /// <param name="request">The create category command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created category DTO.</returns>
        public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating category with name: {Name} by user: {UserId}", request.Name, request.UserId);

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for category creation");

            // Check for duplicate names
            var existingCategory = await _categoryRepository.ExistsByNameAsync(request.Name).ConfigureAwait(false);
            if (existingCategory)
            {
                _logger.LogWarning("Category creation failed: Category with name '{Name}' already exists", request.Name);
                throw new CategoryNameAlreadyExistsException(request.Name);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Create the category
            var category = Category.Create(request.Name, request.Description);

            // Save to repository
            await _categoryRepository.AddAsync(category).ConfigureAwait(false);

            _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

            // Publish domain event
            var categoryCreatedEvent = new CategoryCreatedEvent(category);
            await _mediator.Publish(categoryCreatedEvent, cancellationToken).ConfigureAwait(false);

            // Return DTO
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProductCount = category.BaseProducts?.Count ?? 0
            };
        }
    }
}