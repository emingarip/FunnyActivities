using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a base product that can have multiple variants.
    /// </summary>
    public class BaseProduct
    {
        /// <summary>
        /// Gets the unique identifier of the base product.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the base product.
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the base product.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the category ID of the base product.
        /// </summary>
        public Guid? CategoryId { get; private set; }

        /// <summary>
        /// Gets the category of the base product.
        /// </summary>
        public Category? Category { get; private set; }

        /// <summary>
        /// Gets the date and time when the base product was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the base product was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the collection of variants for this base product.
        /// </summary>
        public ICollection<ProductVariant> Variants { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProduct"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The name of the base product.</param>
        /// <param name="description">The description of the base product.</param>
        /// <param name="categoryId">The category ID.</param>
        public BaseProduct(Guid id, string name, string? description, Guid? categoryId)
        {
            Id = id;
            Name = name;
            Description = description;
            CategoryId = categoryId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Variants = new List<ProductVariant>();
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private BaseProduct() { }

        /// <summary>
        /// Creates a new base product instance.
        /// </summary>
        /// <param name="name">The name of the base product.</param>
        /// <param name="description">The description of the base product.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>A new base product instance.</returns>
        public static BaseProduct Create(string name, string? description, Guid? categoryId)
        {
            return new BaseProduct(Guid.NewGuid(), name, description, categoryId);
        }

        /// <summary>
        /// Updates the details of the base product.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="description">The new description.</param>
        /// <param name="categoryId">The new category ID.</param>
        public void UpdateDetails(string name, string? description, Guid? categoryId)
        {
            Name = name;
            Description = description;
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}