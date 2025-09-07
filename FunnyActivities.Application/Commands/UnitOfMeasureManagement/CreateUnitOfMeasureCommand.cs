using MediatR;
using FunnyActivities.Application.DTOs.UnitOfMeasureManagement;

namespace FunnyActivities.Application.Commands.UnitOfMeasureManagement
{
    /// <summary>
    /// Command for creating a new unit of measure.
    /// </summary>
    public class CreateUnitOfMeasureCommand : IRequest<UnitOfMeasureDto>
    {
        /// <summary>
        /// Gets or sets the name of the unit.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the unit.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the type of the unit.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user creating the unit.
        /// </summary>
        public Guid UserId { get; set; }
    }
}