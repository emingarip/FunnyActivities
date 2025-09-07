using MediatR;
using FunnyActivities.Application.Queries.UnitOfMeasureManagement;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;
using FunnyActivities.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UnitOfMeasureManagement
{
    /// <summary>
    /// Handler for getting a unit of measure by ID.
    /// </summary>
    public class GetUnitOfMeasureQueryHandler : IRequestHandler<GetUnitOfMeasureQuery, UnitOfMeasureDto>
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<GetUnitOfMeasureQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUnitOfMeasureQueryHandler"/> class.
        /// </summary>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public GetUnitOfMeasureQueryHandler(
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<GetUnitOfMeasureQueryHandler> _logger)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            this._logger = _logger;
        }

        /// <summary>
        /// Handles the get unit of measure query.
        /// </summary>
        /// <param name="request">The get unit of measure query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The unit of measure DTO if found; otherwise, null.</returns>
        public async Task<UnitOfMeasureDto> Handle(GetUnitOfMeasureQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving unit of measure with ID: {Id}", request.Id);

            var unitOfMeasure = await _unitOfMeasureRepository.GetByIdAsync(request.Id);

            if (unitOfMeasure == null)
            {
                _logger.LogWarning("Unit of measure with ID {Id} not found", request.Id);
                return null;
            }

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