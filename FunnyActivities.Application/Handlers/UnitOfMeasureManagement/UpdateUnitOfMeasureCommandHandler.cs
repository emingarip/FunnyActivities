using MediatR;
using FunnyActivities.Application.Commands.UnitOfMeasureManagement;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UnitOfMeasureManagement
{
    /// <summary>
    /// Handler for updating a unit of measure.
    /// </summary>
    public class UpdateUnitOfMeasureCommandHandler : IRequestHandler<UpdateUnitOfMeasureCommand, UnitOfMeasureDto>
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<UpdateUnitOfMeasureCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUnitOfMeasureCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public UpdateUnitOfMeasureCommandHandler(
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<UpdateUnitOfMeasureCommandHandler> _logger)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            this._logger = _logger;
        }

        /// <summary>
        /// Handles the update unit of measure command.
        /// </summary>
        /// <param name="request">The update unit of measure command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated unit of measure DTO.</returns>
        public async Task<UnitOfMeasureDto> Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating unit of measure with ID: {Id}", request.Id);

            var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(request.Id);
            if (unitOfMeasure == null)
            {
                _logger.LogWarning("Unit of measure with ID {Id} not found", request.Id);
                throw new KeyNotFoundException($"Unit of measure with ID '{request.Id}' was not found.");
            }

            // Check if another unit with the same name already exists (excluding current unit)
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != unitOfMeasure.Name)
            {
                var existingUnit = await _unitOfMeasureRepository.GetByNameAsync(request.Name);
                if (existingUnit != null && existingUnit.Id != request.Id)
                {
                    _logger.LogWarning("Unit of measure with name '{Name}' already exists", request.Name);
                    throw new ArgumentException($"A unit of measure with the name '{request.Name}' already exists.");
                }
            }

            // Update the unit of measure
            unitOfMeasure.UpdateDetails(
                request.Name ?? unitOfMeasure.Name,
                request.Symbol ?? unitOfMeasure.Symbol,
                request.Type ?? unitOfMeasure.Type
            );

            await _unitOfMeasureRepository.UpdateAsync(unitOfMeasure);

            _logger.LogInformation("Unit of measure updated successfully with ID: {Id}", unitOfMeasure.Id);

            // Return the updated DTO
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