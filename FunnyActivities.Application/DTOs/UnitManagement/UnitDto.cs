using System;

namespace FunnyActivities.Application.DTOs.UnitManagement
{
    /// <summary>
    /// Data transfer object for unit information.
    /// Represents specific units like mm, Litre, kg, etc.
    /// </summary>
    public class UnitDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the unit.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the unit.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the unit type identifier.
        /// </summary>
        public Guid UnitTypeId { get; set; }

        /// <summary>
        /// Gets or sets the unit type name.
        /// </summary>
        public string UnitTypeName { get; set; }

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