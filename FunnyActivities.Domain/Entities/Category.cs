using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a category entity in the system.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Gets the unique identifier of the category.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the category.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the date and time when the category was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the category was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the collection of base products in this category.
        /// </summary>
        public ICollection<BaseProduct> BaseProducts { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The name of the category.</param>
        /// <param name="description">The description of the category.</param>
        public Category(Guid id, string name, string? description)
        {
            Id = id;
            Name = name;
            Description = description;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            BaseProducts = new List<BaseProduct>();
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Category() { }

        /// <summary>
        /// Creates a new category instance.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <param name="description">The description of the category.</param>
        /// <returns>A new category instance.</returns>
        public static Category Create(string name, string? description)
        {
            return new Category(Guid.NewGuid(), name, description);
        }

        /// <summary>
        /// Updates the details of the category.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="description">The new description.</param>
        public void UpdateDetails(string name, string? description)
        {
            Name = name;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}