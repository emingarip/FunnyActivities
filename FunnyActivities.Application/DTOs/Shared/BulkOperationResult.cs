namespace FunnyActivities.Application.DTOs.Shared
{
    /// <summary>
    /// Represents the result of a bulk operation for a single item.
    /// </summary>
    /// <typeparam name="T">The type of the result data.</typeparam>
    public class BulkOperationResult<T>
    {
        /// <summary>
        /// Gets or sets the index of the item in the original request.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the result data if the operation was successful.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation failed.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets additional error details if the operation failed.
        /// </summary>
        public string? ErrorDetails { get; set; }
    }
}