using MediatR;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;

namespace FunnyActivities.Application.Queries.UnitOfMeasureManagement
{
    /// <summary>
    /// Query for getting a unit of measure by ID.
    /// </summary>
    public class GetUnitOfMeasureQuery : IRequest<UnitOfMeasureDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit.
        /// </summary>
        public Guid Id { get; set; }
    }
}