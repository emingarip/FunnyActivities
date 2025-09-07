using System;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Exception thrown when attempting to delete a material that cannot be deleted.
/// </summary>
public class MaterialCannotBeDeletedException : Exception
{
    /// <summary>
    /// Gets the material ID that cannot be deleted.
    /// </summary>
    public Guid MaterialId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialCannotBeDeletedException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID that cannot be deleted.</param>
    public MaterialCannotBeDeletedException(Guid materialId)
        : base($"Material with ID '{materialId}' cannot be deleted because it has dependencies or is in use.")
    {
        MaterialId = materialId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialCannotBeDeletedException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID that cannot be deleted.</param>
    /// <param name="reason">The reason why the material cannot be deleted.</param>
    public MaterialCannotBeDeletedException(Guid materialId, string reason)
        : base($"Material with ID '{materialId}' cannot be deleted: {reason}")
    {
        MaterialId = materialId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialCannotBeDeletedException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID that cannot be deleted.</param>
    /// <param name="reason">The reason why the material cannot be deleted.</param>
    /// <param name="innerException">The inner exception.</param>
    public MaterialCannotBeDeletedException(Guid materialId, string reason, Exception innerException)
        : base($"Material with ID '{materialId}' cannot be deleted: {reason}", innerException)
    {
        MaterialId = materialId;
    }
}