using System;
using System.Collections.Generic;
using System.Linq;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Custom exception for validation errors.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IEnumerable<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">The list of validation errors.</param>
    public ValidationException(IEnumerable<string> errors)
        : base("Validation failed.")
    {
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new List<string> { message };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new List<string> { message };
    }
}