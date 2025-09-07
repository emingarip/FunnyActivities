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
    /// Handler for bulk updating product variants.
    /// </summary>
    public class BulkUpdateProductVariantsCommandHandler : IRequestHandler<BulkUpdateProductVariantsCommand, BulkUpdateProductVariantsResponse>
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<BulkUpdateProductVariantsCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkUpdateProductVariantsCommandHandler"/> class.
        /// </summary>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public BulkUpdateProductVariantsCommandHandler(
            IProductVariantRepository productVariantRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            IMediator mediator,
            ILogger<BulkUpdateProductVariantsCommandHandler> logger)
        {
            _productVariantRepository = productVariantRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the bulk update product variants command.
        /// </summary>
        /// <param name="request">The bulk update product variants command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The bulk update response.</returns>
        public async Task<BulkUpdateProductVariantsResponse> Handle(BulkUpdateProductVariantsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bulk update of {Count} product variants by user: {UserId}", request.Updates.Count, request.UserId);

            var response = new BulkUpdateProductVariantsResponse
            {
                TotalUpdates = request.Updates.Count
            };

            foreach (var updateRequest in request.Updates)
            {
                try
                {
                    var updatedVariant = await UpdateSingleVariantAsync(updateRequest, request.UserId, cancellationToken);
                    if (updatedVariant != null)
                    {
                        response.UpdatedVariants.Add(updatedVariant);
                        response.SuccessfulUpdates++;
                        _logger.LogInformation("Successfully updated variant {VariantId}", updateRequest.Id);
                    }
                }
                catch (Exception ex)
                {
                    var error = new BulkUpdateError
                    {
                        VariantId = updateRequest.Id,
                        ErrorMessage = ex.Message,
                        ErrorType = ex.GetType().Name
                    };
                    response.Errors.Add(error);
                    response.FailedUpdates++;
                    _logger.LogWarning("Failed to update variant {VariantId}: {Error}", updateRequest.Id, ex.Message);
                }
            }

            response.FailedUpdates = response.TotalUpdates - response.SuccessfulUpdates;

            _logger.LogInformation("Bulk update completed. Successful: {Successful}, Failed: {Failed}, Total: {Total}",
                response.SuccessfulUpdates, response.FailedUpdates, response.TotalUpdates);

            return response;
        }

        /// <summary>
        /// Updates a single product variant.
        /// </summary>
        /// <param name="updateRequest">The update request for the variant.</param>
        /// <param name="userId">The ID of the user performing the update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated product variant DTO, or null if update failed.</returns>
        private async Task<ProductVariantDto?> UpdateSingleVariantAsync(ProductVariantUpdateRequest updateRequest, Guid userId, CancellationToken cancellationToken)
        {
            // Get the existing product variant
            var productVariant = await _productVariantRepository.GetByIdAsync(updateRequest.Id);
            if (productVariant == null)
            {
                throw new ProductVariantNotFoundException(updateRequest.Id);
            }

            // Business rule validations
            // Check if unit of measure exists if it's being changed
            if (updateRequest.UnitOfMeasureId.HasValue && updateRequest.UnitOfMeasureId != productVariant.UnitOfMeasureId)
            {
                var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(updateRequest.UnitOfMeasureId.Value);
                if (unitOfMeasure == null)
                {
                    throw new UnitOfMeasureNotFoundException(updateRequest.UnitOfMeasureId.Value);
                }
            }

            // Check for duplicate names within the same base product if name is being changed
            if (updateRequest.Name != null && updateRequest.Name != productVariant.Name)
            {
                var existingVariant = await _productVariantRepository.GetByNameAsync(updateRequest.Name);
                if (existingVariant != null && existingVariant.BaseProductId == productVariant.BaseProductId && existingVariant.Id != updateRequest.Id)
                {
                    throw new ProductVariantNameAlreadyExistsException(updateRequest.Name, productVariant.BaseProductId);
                }
            }

            // Update the product variant
            productVariant.UpdateDetails(
                updateRequest.Name ?? productVariant.Name,
                updateRequest.UnitOfMeasureId ?? productVariant.UnitOfMeasureId,
                updateRequest.UnitValue ?? productVariant.UnitValue,
                updateRequest.UsageNotes ?? productVariant.UsageNotes);

            // Handle dynamic properties
            if (updateRequest.DynamicProperties != null)
            {
                productVariant.UpdateDynamicProperties(updateRequest.DynamicProperties);
            }

            // Save to repository
            await _productVariantRepository.UpdateAsync(productVariant);

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