using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a characteristic/trait of a persona.
    /// </summary>
    public class PersonaCharacteristic
    {
        /// <summary>
        /// Gets the unique identifier of the characteristic.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the persona ID this characteristic belongs to.
        /// </summary>
        public Guid PersonaId { get; private set; }

        /// <summary>
        /// Gets the persona this characteristic belongs to.
        /// </summary>
        public Persona Persona { get; private set; }

        /// <summary>
        /// Gets the name of the characteristic (e.g., "Personality", "Skill").
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the characteristic.
        /// </summary>
        [Required]
        public string Value { get; private set; }

        /// <summary>
        /// Gets the type of the characteristic value (e.g., "string", "int", "bool").
        /// </summary>
        public string? Type { get; private set; }

        /// <summary>
        /// Gets the order/priority of this characteristic.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the date and time when the characteristic was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the characteristic was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaCharacteristic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="personaId">The persona ID.</param>
        /// <param name="name">The name of the characteristic.</param>
        /// <param name="value">The value of the characteristic.</param>
        /// <param name="type">The type of the characteristic value.</param>
        /// <param name="order">The order/priority of the characteristic.</param>
        public PersonaCharacteristic(Guid id, Guid personaId, string name, string value, string? type, int order)
        {
            Id = id;
            PersonaId = personaId;
            Name = name;
            Value = value;
            Type = type;
            Order = order;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private PersonaCharacteristic() { }

        /// <summary>
        /// Creates a new persona characteristic instance.
        /// </summary>
        /// <param name="personaId">The persona ID.</param>
        /// <param name="name">The name of the characteristic.</param>
        /// <param name="value">The value of the characteristic.</param>
        /// <param name="type">The type of the characteristic value.</param>
        /// <param name="order">The order/priority of the characteristic.</param>
        /// <returns>A new persona characteristic instance.</returns>
        public static PersonaCharacteristic Create(Guid personaId, string name, string value, string? type, int order)
        {
            return new PersonaCharacteristic(Guid.NewGuid(), personaId, name, value, type, order);
        }

        /// <summary>
        /// Updates the details of the characteristic.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="value">The new value.</param>
        /// <param name="type">The new type.</param>
        /// <param name="order">The new order.</param>
        public void UpdateDetails(string name, string value, string? type, int order)
        {
            Name = name;
            Value = value;
            Type = type;
            Order = order;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}