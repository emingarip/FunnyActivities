using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Queries.UnitManagement
{
    /// <summary>
    /// Query for retrieving all units.
    /// </summary>
    public class GetUnitsQuery : IRequest<List<UnitDto>>
    {
    }
}