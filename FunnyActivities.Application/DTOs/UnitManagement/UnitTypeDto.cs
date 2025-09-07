using System;

namespace FunnyActivities.Application.DTOs.UnitManagement
{
    /// <summary>
    /// Data transfer object for unit type information.
    /// Represents categories of units like Length, Volume, Weight, etc.
    /// </summary>
    public class UnitTypeDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the unit type.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the unit type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the unit type.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the unit type was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the unit type was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}