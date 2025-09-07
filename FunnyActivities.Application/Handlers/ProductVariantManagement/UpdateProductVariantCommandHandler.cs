using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.ProductVariantManagement
{
    /// <summary>
    /// Handler for updating a product variant.
    /// </summary>
    public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateProductVariantCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProductVariantCommandHandler"/> class.
        /// </summary>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public UpdateProductVariantCommandHandler(
            IProductVariantRepository productVariantRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            IMediator mediator,
            ILogger<UpdateProductVariantCommandHandler> logger)
        {
            _productVariantRepository = productVariantRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update product variant command.
        /// </summary>
        /// <param name="request">The update product variant command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated product variant DTO.</returns>
        public async Task<ProductVariantDto> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product variant with ID: {ProductVariantId} by user: {UserId}", request.Id, request.UserId);

            // Get the existing product variant
            var productVariant = await _productVariantRepository.GetByIdAsync(request.Id);
            if (productVariant == null)
            {
                _logger.LogWarning("Product variant update failed: Product variant with ID '{ProductVariantId}' not found", request.Id);
                throw new ProductVariantNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for product variant update");

            // Check if unit of measure exists if it's being changed
            if (request.UnitOfMeasureId.HasValue && request.UnitOfMeasureId != productVariant.UnitOfMeasureId)
            {
                var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(request.UnitOfMeasureId.Value);
                if (unitOfMeasure == null)
                {
                    _logger.LogWarning("Product variant update failed: Unit of measure with ID '{UnitOfMeasureId}' not found", request.UnitOfMeasureId.Value);
                    throw new UnitOfMeasureNotFoundException(request.UnitOfMeasureId.Value);
                }
            }

            // Check for duplicate names within the same base product if name is being changed
            if (request.Name != productVariant.Name)
            {
                var existingVariant = await _productVariantRepository.GetByNameAsync(request.Name);
                if (existingVariant != null && existingVariant.BaseProductId == productVariant.BaseProductId && existingVariant.Id != request.Id)
                {
                    _logger.LogWarning("Product variant update failed: Product variant with name '{Name}' already exists for base product '{BaseProductId}'", request.Name, productVariant.BaseProductId);
                    throw new ProductVariantNameAlreadyExistsException(request.Name, productVariant.BaseProductId);
                }
            }

            // Update the product variant
            productVariant.UpdateDetails(
                request.Name ?? productVariant.Name,
                request.UnitOfMeasureId ?? productVariant.UnitOfMeasureId,
                request.UnitValue ?? productVariant.UnitValue,
                request.UsageNotes ?? productVariant.UsageNotes);

            // Handle dynamic properties
            if (request.DynamicProperties != null)
            {
                productVariant.UpdateDynamicProperties(request.DynamicProperties);
            }

            // Save to repository
            await _productVariantRepository.UpdateAsync(productVariant);

            _logger.LogInformation("Product variant updated successfully with ID: {ProductVariantId}", productVariant.Id);

            // Publish domain event
            var productVariantUpdatedEvent = new ProductVariantUpdatedEvent(productVariant);
            await _mediator.Publish(productVariantUpdatedEvent, cancellationToken);

            // Return DTO
            return new ProductVariantDto
            {
                Id = productVariant.Id,
                BaseProductId = productVariant.BaseProductId,
                BaseProductName = productVariant.BaseProduct?.Name,
                BaseProductDescription = productVariant.BaseProduct?.Description,
                BaseProductCategoryId = productVariant.BaseProduct?.CategoryId,
                BaseProductCategoryName = productVariant.BaseProduct?.Category?.Name,
                Name = productVariant.Name,
                StockQuantity = productVariant.StockQuantity,
                UnitOfMeasureId = productVariant.UnitOfMeasureId,
                UnitOfMeasureName = productVariant.UnitOfMeasure?.Name,
                UnitSymbol = productVariant.UnitOfMeasure?.Symbol,
                UnitValue = productVariant.UnitValue,
                UsageNotes = productVariant.UsageNotes,
                Photos = productVariant.Photos,
                DynamicProperties = productVariant.DynamicProperties,
                CreatedAt = productVariant.CreatedAt,
                UpdatedAt = productVariant.UpdatedAt
            };
        }
    }
}