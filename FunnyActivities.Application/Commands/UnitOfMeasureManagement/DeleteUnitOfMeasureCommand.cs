using MediatR;

namespace FunnyActivities.Application.Commands.UnitOfMeasureManagement
{
    /// <summary>
    /// Command for deleting a unit of measure.
    /// </summary>
    public class DeleteUnitOfMeasureCommand : IRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit to delete.
        /// </summary>
        public Guid Id { get; set; }
    }
}