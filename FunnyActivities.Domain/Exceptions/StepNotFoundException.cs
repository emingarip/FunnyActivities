using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a step is not found.
    /// </summary>
    public class StepNotFoundException : Exception
    {
        /// <summary>
        /// Gets the ID of the step that was not found.
        /// </summary>
        public Guid StepId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepNotFoundException"/> class.
        /// </summary>
        /// <param name="stepId">The ID of the step that was not found.</param>
        public StepNotFoundException(Guid stepId)
            : base($"Step with ID '{stepId}' was not found.")
        {
            StepId = stepId;
        }
    }
}