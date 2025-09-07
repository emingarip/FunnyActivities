using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create a category with a name that already exists.
    /// </summary>
    public class CategoryNameAlreadyExistsException : Exception
    {
        /// <summary>
        /// Gets the name that already exists.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryNameAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="categoryName">The category name that already exists.</param>
        public CategoryNameAlreadyExistsException(string categoryName)
            : base($"A category with the name '{categoryName}' already exists.")
        {
            CategoryName = categoryName;
        }
    }
}