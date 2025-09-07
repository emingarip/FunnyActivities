using System;

namespace FunnyActivities.Domain.Exceptions;

/// <summary>
/// Exception thrown when a base product is not found.
/// </summary>
public class BaseProductNotFoundException : Exception
{
    /// <summary>
    /// Gets the base product ID that was not found.
    /// </summary>
    public Guid BaseProductId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductNotFoundException"/> class.
    /// </summary>
    /// <param name="baseProductId">The base product ID that was not found.</param>
    public BaseProductNotFoundException(Guid baseProductId)
        : base($"Base product with ID '{baseProductId}' was not found.")
    {
        BaseProductId = baseProductId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductNotFoundException"/> class.
    /// </summary>
    /// <param name="baseProductId">The base product ID that was not found.</param>
    /// <param name="message">The error message.</param>
    public BaseProductNotFoundException(Guid baseProductId, string message)
        : base(message)
    {
        BaseProductId = baseProductId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductNotFoundException"/> class.
    /// </summary>
    /// <param name="baseProductId">The base product ID that was not found.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BaseProductNotFoundException(Guid baseProductId, string message, Exception innerException)
        : base(message, innerException)
    {
        BaseProductId = baseProductId;
    }
}