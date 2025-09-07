using System;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Exception thrown when a material photo is not found.
/// </summary>
public class MaterialPhotoNotFoundException : Exception
{
    /// <summary>
    /// Gets the material ID.
    /// </summary>
    public Guid MaterialId { get; }

    /// <summary>
    /// Gets the photo URL that was not found.
    /// </summary>
    public string PhotoUrl { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialPhotoNotFoundException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID.</param>
    /// <param name="photoUrl">The photo URL that was not found.</param>
    public MaterialPhotoNotFoundException(Guid materialId, string photoUrl)
        : base($"Photo '{photoUrl}' not found for material with ID '{materialId}'.")
    {
        MaterialId = materialId;
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialPhotoNotFoundException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID.</param>
    /// <param name="photoUrl">The photo URL that was not found.</param>
    /// <param name="message">The error message.</param>
    public MaterialPhotoNotFoundException(Guid materialId, string photoUrl, string message)
        : base(message)
    {
        MaterialId = materialId;
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialPhotoNotFoundException"/> class.
    /// </summary>
    /// <param name="materialId">The material ID.</param>
    /// <param name="photoUrl">The photo URL that was not found.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MaterialPhotoNotFoundException(Guid materialId, string photoUrl, string message, Exception innerException)
        : base(message, innerException)
    {
        MaterialId = materialId;
        PhotoUrl = photoUrl;
    }
}