using MediatR;
using System.Collections.Generic;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;

namespace FunnyActivities.Application.Queries.UnitOfMeasureManagement
{
    /// <summary>
    /// Query for getting all units of measure.
    /// </summary>
    public class GetUnitOfMeasuresQuery : IRequest<IEnumerable<UnitOfMeasureDto>>
    {
    }
}