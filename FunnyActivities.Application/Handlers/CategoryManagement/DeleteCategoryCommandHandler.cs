using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.CategoryManagement
{
    /// <summary>
    /// Handler for deleting a category.
    /// </summary>
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCategoryCommandHandler"/> class.
        /// </summary>
        /// <param name="categoryRepository">The category repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public DeleteCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IMediator mediator,
            ILogger<DeleteCategoryCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete category command.
        /// </summary>
        /// <param name="request">The delete category command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit value indicating the operation completed.</returns>
        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting category with ID: {CategoryId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing category
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
            {
                _logger.LogWarning("Category deletion failed: Category with ID '{CategoryId}' not found", request.Id);
                throw new CategoryNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for category deletion");

            // Note: In a real application, you might want to check if the category has associated products
            // and prevent deletion if it does, or cascade delete, etc.
            // For now, we'll allow deletion.

            // Delete from repository
            await _categoryRepository.DeleteAsync(category);

            _logger.LogInformation("Category deleted successfully with ID: {CategoryId}", category.Id);

            // Publish domain event
            var categoryDeletedEvent = new CategoryDeletedEvent(category);
            await _mediator.Publish(categoryDeletedEvent, cancellationToken);

            return Unit.Value;
        }
    }
}