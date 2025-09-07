using System;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Exception thrown when a material is not found.
/// </summary>
public class MaterialNotFoundException : Exception
{
    /// <summary>
    /// Gets the material ID that was not found.
    /// </summary>
    public Guid MaterialId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialNotFoundException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID that was not found.</param>
    public MaterialNotFoundException(Guid materialId)
        : base($"Material with ID '{materialId}' was not found.")
    {
        MaterialId = materialId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialNotFoundException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID that was not found.</param>
    /// <param name="message">The error message.</param>
    public MaterialNotFoundException(Guid materialId, string message)
        : base(message)
    {
        MaterialId = materialId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialNotFoundException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID that was not found.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MaterialNotFoundException(Guid materialId, string message, Exception innerException)
        : base(message, innerException)
    {
        MaterialId = materialId;
    }
}