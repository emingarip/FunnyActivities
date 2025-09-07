using MediatR;
using FunnyActivities.Application.Queries.UnitOfMeasureManagement;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;
using FunnyActivities.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UnitOfMeasureManagement
{
    /// <summary>
    /// Handler for getting all units of measure.
    /// </summary>
    public class GetUnitOfMeasuresQueryHandler : IRequestHandler<GetUnitOfMeasuresQuery, IEnumerable<UnitOfMeasureDto>>
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<GetUnitOfMeasuresQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUnitOfMeasuresQueryHandler"/> class.
        /// </summary>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public GetUnitOfMeasuresQueryHandler(
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<GetUnitOfMeasuresQueryHandler> _logger)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            this._logger = _logger;
        }

        /// <summary>
        /// Handles the get units of measure query.
        /// </summary>
        /// <param name="request">The get units of measure query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of unit of measure DTOs.</returns>
        public async Task<IEnumerable<UnitOfMeasureDto>> Handle(GetUnitOfMeasuresQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all units of measure");

            var unitsOfMeasure = await _unitOfMeasureRepository.GetAllAsync();

            return unitsOfMeasure.Select(unitOfMeasure => new UnitOfMeasureDto
            {
                Id = unitOfMeasure.Id,
                Name = unitOfMeasure.Name,
                Symbol = unitOfMeasure.Symbol,
                Type = unitOfMeasure.Type,
                CreatedAt = unitOfMeasure.CreatedAt,
                UpdatedAt = unitOfMeasure.UpdatedAt
            });
        }
    }
}