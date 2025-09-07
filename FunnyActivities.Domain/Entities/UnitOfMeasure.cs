using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a unit of measure.
    /// </summary>
    public class UnitOfMeasure
    {
        /// <summary>
        /// Gets the unique identifier of the unit of measure.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the unit (e.g., "Millimeter").
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the symbol of the unit (e.g., "mm").
        /// </summary>
        [Required]
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets the type of the unit (e.g., "Length", "Weight", "Volume").
        /// </summary>
        [Required]
        public string Type { get; private set; }

        /// <summary>
        /// Gets the date and time when the unit was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the unit was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfMeasure"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The name of the unit.</param>
        /// <param name="symbol">The symbol of the unit.</param>
        /// <param name="type">The type of the unit.</param>
        public UnitOfMeasure(Guid id, string name, string symbol, string type)
        {
            Id = id;
            Name = name;
            Symbol = symbol;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private UnitOfMeasure() { }

        /// <summary>
        /// Creates a new unit of measure instance.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="symbol">The symbol of the unit.</param>
        /// <param name="type">The type of the unit.</param>
        /// <returns>A new unit of measure instance.</returns>
        public static UnitOfMeasure Create(string name, string symbol, string type)
        {
            return new UnitOfMeasure(Guid.NewGuid(), name, symbol, type);
        }

        /// <summary>
        /// Updates the details of the unit.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="symbol">The new symbol.</param>
        /// <param name="type">The new type.</param>
        public void UpdateDetails(string name, string symbol, string type)
        {
            Name = name;
            Symbol = symbol;
            Type = type;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}