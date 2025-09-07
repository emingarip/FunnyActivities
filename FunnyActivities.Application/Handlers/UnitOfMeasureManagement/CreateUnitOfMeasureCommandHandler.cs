using MediatR;
using FunnyActivities.Application.Commands.UnitOfMeasureManagement;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UnitOfMeasureManagement
{
    /// <summary>
    /// Handler for creating a new unit of measure.
    /// </summary>
    public class CreateUnitOfMeasureCommandHandler : IRequestHandler<CreateUnitOfMeasureCommand, UnitOfMeasureDto>
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<CreateUnitOfMeasureCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateUnitOfMeasureCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public CreateUnitOfMeasureCommandHandler(
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<CreateUnitOfMeasureCommandHandler> _logger)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            this._logger = _logger;
        }

        /// <summary>
        /// Handles the create unit of measure command.
        /// </summary>
        /// <param name="request">The create unit of measure command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created unit of measure DTO.</returns>
        public async Task<UnitOfMeasureDto> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new unit of measure: {Name}", request.Name);

            // Check if a unit with the same name already exists
            var existingUnit = await _unitOfMeasureRepository.GetByNameAsync(request.Name);
            if (existingUnit != null)
            {
                _logger.LogWarning("Unit of measure with name '{Name}' already exists", request.Name);
                throw new ArgumentException($"A unit of measure with the name '{request.Name}' already exists.");
            }

            // Create the new unit of measure
            var unitOfMeasure = UnitOfMeasure.Create(request.Name, request.Symbol, request.Type);

            // Save to database
            await _unitOfMeasureRepository.AddAsync(unitOfMeasure);

            _logger.LogInformation("Unit of measure created successfully with ID: {Id}", unitOfMeasure.Id);

            // Return the DTO
            return new UnitOfMeasureDto
            {
                Id = unitOfMeasure.Id,
                Name = unitOfMeasure.Name,
                Symbol = unitOfMeasure.Symbol,
                Type = unitOfMeasure.Type,
                CreatedAt = unitOfMeasure.CreatedAt,
                UpdatedAt = unitOfMeasure.UpdatedAt
            };
        }
    }
}