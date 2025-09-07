namespace FunnyActivities.Application.DTOs.UnitManagement
{
    /// <summary>
    /// Request DTO for creating a new unit type.
    /// </summary>
    public class CreateUnitTypeRequest
    {
        /// <summary>
        /// Gets or sets the name of the unit type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the unit type.
        /// </summary>
        public string? Description { get; set; }
    }
}