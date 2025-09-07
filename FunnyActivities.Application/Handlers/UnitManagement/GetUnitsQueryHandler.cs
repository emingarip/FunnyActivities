using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.DTOs.UnitManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.UnitManagement;

namespace FunnyActivities.Application.Handlers.UnitManagement
{
    /// <summary>
    /// Handler for retrieving all units.
    /// </summary>
    public class GetUnitsQueryHandler : IRequestHandler<GetUnitsQuery, List<UnitDto>>
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly ILogger<GetUnitsQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUnitsQueryHandler"/> class.
        /// </summary>
        /// <param name="unitOfMeasureRepository">The unit of measure repository.</param>
        /// <param name="logger">The logger.</param>
        public GetUnitsQueryHandler(
            IUnitOfMeasureRepository unitOfMeasureRepository,
            ILogger<GetUnitsQueryHandler> logger)
        {
            _unitOfMeasureRepository = unitOfMeasureRepository ?? throw new ArgumentNullException(nameof(unitOfMeasureRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the get units query.
        /// </summary>
        /// <param name="request">The get units query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of unit DTOs.</returns>
        public async Task<List<UnitDto>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all units");

            try
            {
                var unitOfMeasures = await _unitOfMeasureRepository.GetAllAsync();

                var unitDtos = unitOfMeasures.Select(uom => new UnitDto
                {
                    Id = uom.Id,
                    Name = uom.Name,
                    Description = uom.Symbol, // Map Symbol to Description
                    UnitTypeId = Guid.Empty, // UnitType entity not implemented yet
                    UnitTypeName = uom.Type, // Map Type to UnitTypeName
                    CreatedAt = uom.CreatedAt,
                    UpdatedAt = uom.UpdatedAt
                }).ToList();

                _logger.LogInformation("Retrieved {Count} units successfully", unitDtos.Count);

                return unitDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving units");
                throw;
            }
        }
    }
}