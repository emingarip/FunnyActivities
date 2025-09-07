using MediatR;

namespace FunnyActivities.Application.Commands.UnitManagement
{
    /// <summary>
    /// Command for deleting a unit type.
    /// </summary>
    public class DeleteUnitTypeCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit type to delete.
        /// </summary>
        public Guid Id { get; set; }
    }
}