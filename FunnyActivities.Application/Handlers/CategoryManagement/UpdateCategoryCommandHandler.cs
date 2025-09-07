using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for updating an existing category.
    /// </summary>
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCategoryCommandHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public UpdateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IMediator mediator,
            ILogger<UpdateCategoryCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update category command.
        /// </summary>
        /// <param name="request">The update category command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated category DTO.</returns>
        public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating category with ID: {CategoryId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing category
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
            {
                _logger.LogWarning("Category update failed: Category with ID '{CategoryId}' not found", request.Id);
                throw new CategoryNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for category update");

            // Check for duplicate names (excluding current category)
            var existingCategory = await _categoryRepository.ExistsByNameExcludingIdAsync(request.Name, request.Id);
            if (existingCategory)
            {
                _logger.LogWarning("Category update failed: Category with name '{Name}' already exists", request.Name);
                throw new CategoryNameAlreadyExistsException(request.Name);
            }

            // Update the category
            category.UpdateDetails(request.Name, request.Description);

            // Save to repository
            await _categoryRepository.UpdateAsync(category);

            _logger.LogInformation("Category updated successfully with ID: {CategoryId}", category.Id);

            // Publish domain event
            var categoryUpdatedEvent = new CategoryUpdatedEvent(category);
            await _mediator.Publish(categoryUpdatedEvent, cancellationToken);

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