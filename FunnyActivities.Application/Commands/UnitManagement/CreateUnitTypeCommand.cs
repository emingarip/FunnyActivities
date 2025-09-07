using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Commands.UnitManagement
{
    /// <summary>
    /// Command for creating a new unit type.
    /// </summary>
    public class CreateUnitTypeCommand : IRequest<UnitTypeDto>
    {
        /// <summary>
        /// Gets or sets the request data for creating a unit type.
        /// </summary>
        public CreateUnitTypeRequest Request { get; set; }
    }
}