using System;

namespace FunnyActivities.Application.DTOs.UnitOfMeasureManagement
{
    /// <summary>
    /// Request DTO for creating a new unit of measure.
    /// </summary>
    public class CreateUnitOfMeasureRequest
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
    }
}