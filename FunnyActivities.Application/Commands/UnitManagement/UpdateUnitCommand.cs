using MediatR;
using FunnyActivities.Application.DTOs.UnitManagement;

namespace FunnyActivities.Application.Commands.UnitManagement
{
    /// <summary>
    /// Command for updating an existing unit.
    /// </summary>
    public class UpdateUnitCommand : IRequest<UnitDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the request data for updating the unit.
        /// </summary>
        public UpdateUnitRequest Request { get; set; }
    }
}