using System;

namespace FunnyActivities.Application.DTOs.UnitManagement
{
    /// <summary>
    /// Request DTO for updating an existing unit.
    /// </summary>
    public class UpdateUnitRequest
    {
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
    }
}