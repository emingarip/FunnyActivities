using System;

namespace FunnyActivities.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to delete a base product that has variants.
/// </summary>
public class BaseProductHasVariantsException : Exception
{
    /// <summary>
    /// Gets the base product ID that has variants.
    /// </summary>
    public Guid BaseProductId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductHasVariantsException"/> class.
    /// </summary>
    /// <param name="baseProductId">The base product ID that has variants.</param>
    public BaseProductHasVariantsException(Guid baseProductId)
        : base($"Base product with ID '{baseProductId}' cannot be deleted because it has associated variants.")
    {
        BaseProductId = baseProductId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductHasVariantsException"/> class.
    /// </summary>
    /// <param name="baseProductId">The base product ID that has variants.</param>
    /// <param name="message">The error message.</param>
    public BaseProductHasVariantsException(Guid baseProductId, string message)
        : base(message)
    {
        BaseProductId = baseProductId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseProductHasVariantsException"/> class.
    /// </summary>
    /// <param name="baseProductId">The base product ID that has variants.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BaseProductHasVariantsException(Guid baseProductId, string message, Exception innerException)
        : base(message, innerException)
    {
        BaseProductId = baseProductId;
    }
}