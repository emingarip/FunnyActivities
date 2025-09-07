using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Commands.UnitManagement
{
    /// <summary>
    /// Command for creating a new unit.
    /// </summary>
    public class CreateUnitCommand : IRequest<UnitDto>
    {
        /// <summary>
        /// Gets or sets the request data for creating a unit.
        /// </summary>
        public CreateUnitRequest Request { get; set; }
    }
}