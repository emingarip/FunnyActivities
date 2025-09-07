namespace FunnyActivities.Application.DTOs.Shared
{
    /// <summary>
    /// Standardized API response wrapper for consistent response format across all endpoints.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned in the response.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A message describing the result of the operation.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The actual data returned by the operation. Null for error responses.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        /// <param name="data">The data to include in the response.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>A successful ApiResponse instance.</returns>
        public static ApiResponse<T> CreateSuccess(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Creates a successful response without data (for operations like DELETE).
        /// </summary>
        /// <param name="message">Optional success message.</param>
        /// <returns>A successful ApiResponse instance with null data.</returns>
        public static ApiResponse<T> CreateSuccess(string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// Standardized API error response wrapper for consistent error format across all endpoints.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Indicates whether the operation was successful (always false for errors).
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// A message describing the error that occurred.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The type or category of the error.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Creates an error response.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="error">The error type or category.</param>
        /// <returns>An ApiErrorResponse instance.</returns>
        public static ApiErrorResponse CreateError(string message, string error = "Error")
        {
            return new ApiErrorResponse
            {
                Message = message,
                Error = error
            };
        }
    }
}