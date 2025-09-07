using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Queries.UnitManagement
{
    /// <summary>
    /// Query for retrieving a single unit type by ID.
    /// </summary>
    public class GetUnitTypeQuery : IRequest<UnitTypeDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit type to retrieve.
        /// </summary>
        public Guid Id { get; set; }
    }
}