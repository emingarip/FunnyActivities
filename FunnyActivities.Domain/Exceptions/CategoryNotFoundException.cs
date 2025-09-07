using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a category is not found.
    /// </summary>
    public class CategoryNotFoundException : Exception
    {
        /// <summary>
        /// Gets the ID of the category that was not found.
        /// </summary>
        public Guid CategoryId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryNotFoundException"/> class.
        /// </summary>
        /// <param name="categoryId">The ID of the category that was not found.</param>
        public CategoryNotFoundException(Guid categoryId)
            : base($"Category with ID '{categoryId}' was not found.")
        {
            CategoryId = categoryId;
        }
    }
}