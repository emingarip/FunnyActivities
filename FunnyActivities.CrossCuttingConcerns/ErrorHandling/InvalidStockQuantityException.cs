using System;

namespace FunnyActivities.CrossCuttingConcerns.ErrorHandling;

/// <summary>
/// Exception thrown when attempting to set an invalid stock quantity.
/// </summary>
public class InvalidStockQuantityException : Exception
{
    /// <summary>
    /// Gets the invalid stock quantity value.
    /// </summary>
    public decimal StockQuantity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStockQuantityException"/> class.
    /// </summary>
    /// <param name="stockQuantity">The invalid stock quantity.</param>
    public InvalidStockQuantityException(decimal stockQuantity)
        : base($"Invalid stock quantity: {stockQuantity}. Stock quantity cannot be negative.")
    {
        StockQuantity = stockQuantity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStockQuantityException"/> class.
    /// </summary>
    /// <param name="stockQuantity">The invalid stock quantity.</param>
    /// <param name="message">The error message.</param>
    public InvalidStockQuantityException(decimal stockQuantity, string message)
        : base(message)
    {
        StockQuantity = stockQuantity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStockQuantityException"/> class.
    /// </summary>
    /// <param name="stockQuantity">The invalid stock quantity.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidStockQuantityException(decimal stockQuantity, string message, Exception innerException)
        : base(message, innerException)
    {
        StockQuantity = stockQuantity;
    }
}