using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Queries.UnitManagement
{
    /// <summary>
    /// Query for retrieving a single unit by ID.
    /// </summary>
    public class GetUnitQuery : IRequest<UnitDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit to retrieve.
        /// </summary>
        public Guid Id { get; set; }
    }
}