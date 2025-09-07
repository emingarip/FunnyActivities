using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Queries.UnitManagement
{
    /// <summary>
    /// Query for retrieving all unit types.
    /// </summary>
    public class GetUnitTypesQuery : IRequest<List<UnitTypeDto>>
    {
    }
}