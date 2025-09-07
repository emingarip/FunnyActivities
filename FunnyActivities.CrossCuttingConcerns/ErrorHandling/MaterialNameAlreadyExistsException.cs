using System;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Exception thrown when attempting to create or update a material with a name that already exists.
/// </summary>
public class MaterialNameAlreadyExistsException : Exception
{
    /// <summary>
    /// Gets the name that already exists.
    /// </summary>
    public string MaterialName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialNameAlreadyExistsException"/> class.
    /// </summary>
    /// <param name="materialName">The material name that already exists.</param>
    public MaterialNameAlreadyExistsException(string materialName)
        : base($"A material with the name '{materialName}' already exists.")
    {
        MaterialName = materialName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialNameAlreadyExistsException"/> class.
    /// </summary>
    /// <param name="materialName">The material name that already exists.</param>
    /// <param name="innerException">The inner exception.</param>
    public MaterialNameAlreadyExistsException(string materialName, Exception innerException)
        : base($"A material with the name '{materialName}' already exists.", innerException)
    {
        MaterialName = materialName;
    }
}