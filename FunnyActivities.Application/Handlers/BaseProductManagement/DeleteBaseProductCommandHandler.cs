using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.BaseProductManagement
{
    /// <summary>
    /// Handler for deleting a base product.
    /// </summary>
    public class DeleteBaseProductCommandHandler : IRequestHandler<DeleteBaseProductCommand, Unit>
    {
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteBaseProductCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteBaseProductCommandHandler"/> class.
        /// </summary>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public DeleteBaseProductCommandHandler(
            IBaseProductRepository baseProductRepository,
            IProductVariantRepository productVariantRepository,
            IMediator mediator,
            ILogger<DeleteBaseProductCommandHandler> logger)
        {
            _baseProductRepository = baseProductRepository;
            _productVariantRepository = productVariantRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the delete base product command.
        /// </summary>
        /// <param name="request">The delete base product command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unit value indicating the operation completed.</returns>
        public async Task<Unit> Handle(DeleteBaseProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting base product with ID: {BaseProductId} by user: {UserId}", request.Id, request.UserId);

            // Retrieve the base product
            var baseProduct = await _baseProductRepository.GetByIdAsync(request.Id);
            if (baseProduct == null)
            {
                _logger.LogWarning("Base product deletion failed: Base product with ID {BaseProductId} not found", request.Id);
                throw new BaseProductNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for base product deletion");

            // Check if base product has variants
            var variants = await _productVariantRepository.GetByBaseProductIdAsync(request.Id);
            var hasVariants = variants.Any();
            var variantCount = variants.Count();

            _logger.LogInformation("Base product {BaseProductId} has {VariantCount} variants, CascadeDeleteVariants: {CascadeDeleteVariants}",
                request.Id, variantCount, request.CascadeDeleteVariants);
            _logger.LogInformation("Handler: Processing delete with CascadeDeleteVariants={CascadeDeleteVariants} for product {BaseProductId}",
                request.CascadeDeleteVariants, request.Id);

            if (hasVariants)
            {
                if (request.CascadeDeleteVariants)
                {
                    // Cascade delete variants
                    _logger.LogInformation("Starting cascade delete of {VariantCount} variants for base product {BaseProductId}", variantCount, request.Id);
                    foreach (var variant in variants)
                    {
                        await _productVariantRepository.DeleteAsync(variant);
                        _logger.LogInformation("Deleted variant with ID: {VariantId} for base product {BaseProductId}", variant.Id, request.Id);
                    }
                    _logger.LogInformation("Cascade delete completed for base product {BaseProductId}", request.Id);
                }
                else
                {
                    _logger.LogWarning("Base product deletion failed: Base product with ID {BaseProductId} has associated variants", request.Id);
                    throw new BaseProductHasVariantsException(request.Id);
                }
            }
            else
            {
                _logger.LogInformation("Base product {BaseProductId} has no variants, proceeding with deletion", request.Id);
            }

            // Delete the base product
            await _baseProductRepository.DeleteAsync(baseProduct);

            _logger.LogInformation("Base product deleted successfully with ID: {BaseProductId}", baseProduct.Id);

            // Publish domain event
            var baseProductDeletedEvent = new BaseProductDeletedEvent(baseProduct);
            await _mediator.Publish(baseProductDeletedEvent, cancellationToken);

            return Unit.Value;
        }
    }
}