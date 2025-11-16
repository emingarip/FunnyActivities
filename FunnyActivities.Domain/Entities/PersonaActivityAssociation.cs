using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents the association between a persona and an activity.
    /// </summary>
    public class PersonaActivityAssociation
    {
        /// <summary>
        /// Gets the unique identifier of the association.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the persona ID.
        /// </summary>
        public Guid PersonaId { get; private set; }

        /// <summary>
        /// Gets the persona.
        /// </summary>
        public Persona Persona { get; private set; }

        /// <summary>
        /// Gets the activity ID.
        /// </summary>
        public Guid ActivityId { get; private set; }

        /// <summary>
        /// Gets the activity.
        /// </summary>
        public Activity Activity { get; private set; }


        /// <summary>
        /// Gets the date and time when the association was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the association was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaActivityAssociation"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="personaId">The persona ID.</param>
        /// <param name="activityId">The activity ID.</param>
        public PersonaActivityAssociation(Guid id, Guid personaId, Guid activityId)
        {
            Id = id;
            PersonaId = personaId;
            ActivityId = activityId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private PersonaActivityAssociation() { }

        /// <summary>
        /// Creates a new persona activity association instance.
        /// </summary>
        /// <param name="personaId">The persona ID.</param>
        /// <param name="activityId">The activity ID.</param>
        /// <returns>A new persona activity association instance.</returns>
        public static PersonaActivityAssociation Create(Guid personaId, Guid activityId)
        {
            return new PersonaActivityAssociation(Guid.NewGuid(), personaId, activityId);
        }

    }
}