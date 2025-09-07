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
    /// Handler for creating a new product variant.
    /// </summary>
    public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateProductVariantCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProductVariantCommandHandler"/> class.
        /// </summary>
        /// <param name="productVariantRepository">The product variant repository.</param>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public CreateProductVariantCommandHandler(
            IProductVariantRepository productVariantRepository,
            IBaseProductRepository baseProductRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            IMediator mediator,
            ILogger<CreateProductVariantCommandHandler> logger)
        {
            _productVariantRepository = productVariantRepository;
            _baseProductRepository = baseProductRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create product variant command.
        /// </summary>
        /// <param name="request">The create product variant command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created product variant DTO.</returns>
        public async Task<ProductVariantDto> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating product variant with name: {Name} for base product: {BaseProductId} by user: {UserId}", request.Name, request.BaseProductId, request.UserId);

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for product variant creation");

            // Check if base product exists
            var baseProduct = await _baseProductRepository.GetByIdAsync(request.BaseProductId);
            if (baseProduct == null)
            {
                _logger.LogWarning("Product variant creation failed: Base product with ID '{BaseProductId}' not found", request.BaseProductId);
                throw new BaseProductNotFoundException(request.BaseProductId);
            }

            // Check if unit of measure exists
            var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(request.UnitOfMeasureId);
            if (unitOfMeasure == null)
            {
                _logger.LogWarning("Product variant creation failed: Unit of measure with ID '{UnitOfMeasureId}' not found", request.UnitOfMeasureId);
                throw new UnitOfMeasureNotFoundException(request.UnitOfMeasureId);
            }

            // Check for duplicate names within the same base product
            var existingVariant = await _productVariantRepository.GetByNameAsync(request.Name);
            if (existingVariant != null && existingVariant.BaseProductId == request.BaseProductId)
            {
                _logger.LogWarning("Product variant creation failed: Product variant with name '{Name}' already exists for base product '{BaseProductId}'", request.Name, request.BaseProductId);
                throw new ProductVariantNameAlreadyExistsException(request.Name, request.BaseProductId);
            }

            // Create the product variant
            var productVariant = ProductVariant.Create(
                request.BaseProductId,
                request.Name,
                request.StockQuantity,
                request.UnitOfMeasureId,
                request.UnitValue,
                request.UsageNotes);

            // Handle photo files if provided
            if (request.PhotoFiles != null && request.PhotoFiles.Any())
            {
                // TODO: Implement photo upload logic
                _logger.LogInformation("Photo files provided but upload logic not implemented yet");
            }

            // Handle dynamic properties
            if (request.DynamicProperties != null)
            {
                productVariant.UpdateDynamicProperties(request.DynamicProperties);
            }

            // Save to repository
            await _productVariantRepository.AddAsync(productVariant);

            _logger.LogInformation("Product variant created successfully with ID: {ProductVariantId}", productVariant.Id);

            // Publish domain event
            var productVariantCreatedEvent = new ProductVariantCreatedEvent(productVariant);
            await _mediator.Publish(productVariantCreatedEvent, cancellationToken);

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