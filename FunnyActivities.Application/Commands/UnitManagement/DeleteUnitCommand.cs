using MediatR;

namespace FunnyActivities.Application.Commands.UnitManagement
{
    /// <summary>
    /// Command for deleting a unit.
    /// </summary>
    public class DeleteUnitCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit to delete.
        /// </summary>
        public Guid Id { get; set; }
    }
}