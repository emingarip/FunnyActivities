using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a unit of measure is not found.
    /// </summary>
    public class UnitOfMeasureNotFoundException : Exception
    {
        /// <summary>
        /// Gets the ID of the unit of measure that was not found.
        /// </summary>
        public Guid UnitOfMeasureId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfMeasureNotFoundException"/> class.
        /// </summary>
        /// <param name="unitOfMeasureId">The ID of the unit of measure that was not found.</param>
        public UnitOfMeasureNotFoundException(Guid unitOfMeasureId)
            : base($"Unit of measure with ID '{unitOfMeasureId}' was not found.")
        {
            UnitOfMeasureId = unitOfMeasureId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfMeasureNotFoundException"/> class.
        /// </summary>
        /// <param name="unitOfMeasureId">The ID of the unit of measure that was not found.</param>
        /// <param name="message">The error message.</param>
        public UnitOfMeasureNotFoundException(Guid unitOfMeasureId, string message)
            : base(message)
        {
            UnitOfMeasureId = unitOfMeasureId;
        }
    }
}