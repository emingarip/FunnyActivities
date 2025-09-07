using System;

namespace FunnyActivities.Application.DTOs.UnitOfMeasureManagement
{
    /// <summary>
    /// Data transfer object for unit of measure information.
    /// </summary>
    public class UnitOfMeasureDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit of measure.
        /// </summary>
        public Guid Id { get; set; }

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
        /// Gets or sets the date and time when the unit was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the unit was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}