using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Commands.UnitManagement
{
    /// <summary>
    /// Command for updating an existing unit type.
    /// </summary>
    public class UpdateUnitTypeCommand : IRequest<UnitTypeDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit type to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the request data for updating the unit type.
        /// </summary>
        public UpdateUnitTypeRequest Request { get; set; }
    }
}