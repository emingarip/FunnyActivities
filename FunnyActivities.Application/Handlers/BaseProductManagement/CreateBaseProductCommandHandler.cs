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
    /// Handler for creating a new base product.
    /// </summary>
    public class CreateBaseProductCommandHandler : IRequestHandler<CreateBaseProductCommand, BaseProductDto>
    {
        private readonly IBaseProductRepository _baseProductRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateBaseProductCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBaseProductCommandHandler"/> class.
        /// </summary>
        /// <param name="baseProductRepository">The base product repository.</param>
        /// <param name="mediator">The mediator for publishing events.</param>
        /// <param name="logger">The logger.</param>
        public CreateBaseProductCommandHandler(
            IBaseProductRepository baseProductRepository,
            IMediator mediator,
            ILogger<CreateBaseProductCommandHandler> logger)
        {
            _baseProductRepository = baseProductRepository;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the create base product command.
        /// </summary>
        /// <param name="request">The create base product command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created base product DTO.</returns>
        public async Task<BaseProductDto> Handle(CreateBaseProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating base product with name: {Name} by user: {UserId}", request.Name, request.UserId);

            // Business rule validations
            _logger.LogInformation("Performing business rule validations for base product creation");

            // Check for duplicate names
            var existingBaseProduct = await _baseProductRepository.ExistsByNameAsync(request.Name);
            if (existingBaseProduct)
            {
                _logger.LogWarning("Base product creation failed: Base product with name '{Name}' already exists", request.Name);
                throw new BaseProductNameAlreadyExistsException(request.Name);
            }

            // Create the base product
            var baseProduct = BaseProduct.Create(request.Name, request.Description, request.CategoryId);

            // Save to repository
            await _baseProductRepository.AddAsync(baseProduct);

            _logger.LogInformation("Base product created successfully with ID: {BaseProductId}", baseProduct.Id);

            // Publish domain event
            var baseProductCreatedEvent = new BaseProductCreatedEvent(baseProduct);
            await _mediator.Publish(baseProductCreatedEvent, cancellationToken);

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