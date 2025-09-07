using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create an activity category with a name that already exists.
    /// </summary>
    public class ActivityCategoryNameAlreadyExistsException : Exception
    {
        /// <summary>
        /// Gets the name that already exists.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCategoryNameAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="name">The name that already exists.</param>
        public ActivityCategoryNameAlreadyExistsException(string name)
            : base($"Activity category with name '{name}' already exists.")
        {
            Name = name;
        }
    }
}