using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Application.DTOs.BaseProductManagement;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.Handlers.BaseProductManagement
{
    /// <summary>
    /// Handler for updating an existing base product.
    /// </summary>
    public class UpdateBaseProductCommandHandler : IRequestHandler<UpdateBaseProductCommand, BaseProductDto>
    {
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateBaseProductCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBaseProductCommandHandler"/> class.
        /// </summary>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public UpdateBaseProductCommandHandler(
            IBaseProductRepository baseProductRepository,
            IMediator mediator,
            ILogger<UpdateBaseProductCommandHandler> logger)
        {
            _baseProductRepository = baseProductRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the update base product command.
        /// </summary>
        /// <param name="request">The update base product command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated base product DTO.</returns>
        public async Task<BaseProductDto> Handle(UpdateBaseProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating base product with ID: {BaseProductId} by user: {UserId}", request.Id, request.UserId);

            // Retrieve the base product
            var baseProduct = await _baseProductRepository.GetByIdAsync(request.Id);
            if (baseProduct == null)
            {
                _logger.LogWarning("Base product update failed: Base product with ID {BaseProductId} not found", request.Id);
                throw new BaseProductNotFoundException(request.Id);
            }

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for base product update");

            // Check for duplicate names when name is being updated (excluding current base product)
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != baseProduct.Name)
            {
                var existingBaseProduct = await _baseProductRepository.ExistsByNameExcludingIdAsync(request.Name, request.Id);
                if (existingBaseProduct)
                {
                    _logger.LogWarning("Base product update failed: Base product with name '{Name}' already exists", request.Name);
                    throw new BaseProductNameAlreadyExistsException(request.Name);
                }
            }

            // Update the base product
            baseProduct.UpdateDetails(
                request.Name ?? baseProduct.Name,
                request.Description ?? baseProduct.Description,
                request.CategoryId ?? baseProduct.CategoryId);

            // Save changes
            await _baseProductRepository.UpdateAsync(baseProduct);

            _logger.LogInformation("Base product updated successfully with ID: {BaseProductId}", baseProduct.Id);

            // Publish domain event
            var baseProductUpdatedEvent = new BaseProductUpdatedEvent(baseProduct);
            await _mediator.Publish(baseProductUpdatedEvent, cancellationToken);

            // Return DTO
            return new BaseProductDto
            {
                Id = baseProduct.Id,
                Name = baseProduct.Name,
                Description = baseProduct.Description,
                CategoryId = baseProduct.CategoryId,
                CategoryName = baseProduct.Category?.Name,
                CreatedAt = baseProduct.CreatedAt,
                UpdatedAt = baseProduct.UpdatedAt
            };
        }
    }
}