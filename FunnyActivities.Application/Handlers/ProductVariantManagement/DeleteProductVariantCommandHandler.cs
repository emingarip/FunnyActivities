using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ProductVariantManagement
{
    /// <summary>
    /// Handler for deleting a product variant.
    /// </summary>
    public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Unit>
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteProductVariantCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteProductVariantCommandHandler"/> class.
        /// </summary>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public DeleteProductVariantCommandHandler(
            IProductVariantRepository productVariantRepository,
            IMediator mediator,
            ILogger<DeleteProductVariantCommandHandler> logger)
        {
            _productVariantRepository = productVariantRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete product variant command.
        /// </summary>
        /// <param name="request">The delete product variant command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<Unit> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting product variant with ID: {ProductVariantId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing product variant
            var productVariant = await _productVariantRepository.GetByIdAsync(request.Id);
            if (productVariant == null)
            {
                _logger.LogWarning("Product variant deletion failed: Product variant with ID '{ProductVariantId}' not found", request.Id);
                throw new ProductVariantNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for product variant deletion");

            // Check if the variant has stock (optional business rule)
            if (productVariant.StockQuantity > 0)
            {
                _logger.LogWarning("Product variant deletion failed: Cannot delete product variant with stock quantity '{StockQuantity}'", productVariant.StockQuantity);
                throw new InvalidOperationException("Cannot delete a product variant that has stock.");
            }

            // Delete the product variant
            await _productVariantRepository.DeleteAsync(productVariant);

            _logger.LogInformation("Product variant deleted successfully with ID: {ProductVariantId}", productVariant.Id);

            // Publish domain event
            var productVariantDeletedEvent = new ProductVariantDeletedEvent(productVariant);
            await _mediator.Publish(productVariantDeletedEvent, cancellationToken);

            return Unit.Value;
        }
    }
}