using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a persona/avatar for a user.
    /// </summary>
    public class Persona
    {
        /// <summary>
        /// Gets the unique identifier of the persona.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the user ID that owns this persona.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the user that owns this persona.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets the name of the persona.
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the persona.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the avatar image URL of the persona.
        /// </summary>
        public string? AvatarImageUrl { get; private set; }

        /// <summary>
        /// Gets the age of the persona.
        /// </summary>
        public int? Age { get; private set; }

        /// <summary>
        /// Gets the gender of the persona.
        /// </summary>
        public Gender? Gender { get; private set; }

        /// <summary>
        /// Gets the nationality of the persona.
        /// </summary>
        public string? Nationality { get; private set; }

        /// <summary>
        /// Gets the biography of the persona.
        /// </summary>
        public string? Biography { get; private set; }

        /// <summary>
        /// Gets the list of characteristics for this persona.
        /// </summary>
        public List<PersonaCharacteristic> Characteristics { get; private set; }

        /// <summary>
        /// Gets the list of activity associations for this persona.
        /// </summary>
        public List<PersonaActivityAssociation> ActivityAssociations { get; private set; }

        /// <summary>
        /// Gets the list of images for this persona.
        /// </summary>
        public List<Image> Images { get; private set; }

        /// <summary>
        /// Gets the date and time when the persona was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the persona was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Persona"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="name">The name of the persona.</param>
        /// <param name="description">The description of the persona.</param>
        /// <param name="avatarImageUrl">The avatar image URL.</param>
        /// <param name="age">The age of the persona.</param>
        /// <param name="gender">The gender of the persona.</param>
        /// <param name="nationality">The nationality of the persona.</param>
        /// <param name="biography">The biography of the persona.</param>
        public Persona(Guid id, Guid userId, string name, string? description, string? avatarImageUrl, int? age, Gender? gender, string? nationality, string? biography)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Description = description;
            AvatarImageUrl = avatarImageUrl;
            Age = age;
            Gender = gender;
            Nationality = nationality;
            Biography = biography;
            Characteristics = new List<PersonaCharacteristic>();
            ActivityAssociations = new List<PersonaActivityAssociation>();
            Images = new List<Image>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Persona()
        {
            Characteristics = new List<PersonaCharacteristic>();
            ActivityAssociations = new List<PersonaActivityAssociation>();
            Images = new List<Image>();
        }

        /// <summary>
        /// Creates a new persona instance.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="name">The name of the persona.</param>
        /// <param name="description">The description of the persona.</param>
        /// <param name="avatarImageUrl">The avatar image URL.</param>
        /// <param name="age">The age of the persona.</param>
        /// <param name="gender">The gender of the persona.</param>
        /// <param name="nationality">The nationality of the persona.</param>
        /// <param name="biography">The biography of the persona.</param>
        /// <returns>A new persona instance.</returns>
        public static Persona Create(Guid userId, string name, string? description, string? avatarImageUrl, int? age, Gender? gender, string? nationality, string? biography)
        {
            return new Persona(Guid.NewGuid(), userId, name, description, avatarImageUrl, age, gender, nationality, biography);
        }

        /// <summary>
        /// Updates the details of the persona.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="description">The new description.</param>
        /// <param name="avatarImageUrl">The new avatar image URL.</param>
        /// <param name="age">The new age.</param>
        /// <param name="gender">The new gender.</param>
        /// <param name="nationality">The new nationality.</param>
        /// <param name="biography">The new biography.</param>
        public void UpdateDetails(string name, string? description, string? avatarImageUrl, int? age, Gender? gender, string? nationality, string? biography)
        {
            Name = name;
            Description = description;
            AvatarImageUrl = avatarImageUrl;
            Age = age;
            Gender = gender;
            Nationality = nationality;
            Biography = biography;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a characteristic to the persona.
        /// </summary>
        /// <param name="characteristic">The characteristic to add.</param>
        public void AddCharacteristic(PersonaCharacteristic characteristic)
        {
            Characteristics ??= new List<PersonaCharacteristic>();
            Characteristics.Add(characteristic);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes a characteristic from the persona.
        /// </summary>
        /// <param name="characteristic">The characteristic to remove.</param>
        public void RemoveCharacteristic(PersonaCharacteristic characteristic)
        {
            Characteristics?.Remove(characteristic);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds an activity association to the persona.
        /// </summary>
        /// <param name="association">The activity association to add.</param>
        public void AddActivityAssociation(PersonaActivityAssociation association)
        {
            ActivityAssociations ??= new List<PersonaActivityAssociation>();
            ActivityAssociations.Add(association);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes an activity association from the persona.
        /// </summary>
        /// <param name="association">The activity association to remove.</param>
        public void RemoveActivityAssociation(PersonaActivityAssociation association)
        {
            ActivityAssociations?.Remove(association);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
