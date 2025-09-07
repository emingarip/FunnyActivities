using System;

namespace FunnyActivities.Domain.Exceptions;

/// <summary>
/// Exception thrown when a base product name already exists.
/// </summary>
public class BaseProductNameAlreadyExistsException : Exception
{
    /// <summary>
    /// Gets the base product name that already exists.
    /// </summary>
    public string BaseProductName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductNameAlreadyExistsException"/> class.
    /// </summary>
    /// <param name="baseProductName">The base product name that already exists.</param>
    public BaseProductNameAlreadyExistsException(string baseProductName)
        : base($"Base product with name '{baseProductName}' already exists.")
    {
        BaseProductName = baseProductName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductNameAlreadyExistsException"/> class.
    /// </summary>
    /// <param name="baseProductName">The base product name that already exists.</param>
    /// <param name="message">The error message.</param>
    public BaseProductNameAlreadyExistsException(string baseProductName, string message)
        : base(message)
    {
        BaseProductName = baseProductName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductNameAlreadyExistsException"/> class.
    /// </summary>
    /// <param name="baseProductName">The base product name that already exists.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BaseProductNameAlreadyExistsException(string baseProductName, string message, Exception innerException)
        : base(message, innerException)
    {
        BaseProductName = baseProductName;
    }
}