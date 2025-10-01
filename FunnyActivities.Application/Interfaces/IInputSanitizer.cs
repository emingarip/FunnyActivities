namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Interface for input sanitization services to prevent XSS attacks and improve data quality.
    /// </summary>
    public interface IInputSanitizer
    {
        /// <summary>
        /// Sanitizes a string input by removing potentially dangerous HTML/script content.
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <returns>The sanitized string.</returns>
        string SanitizeString(string input);

        /// <summary>
        /// Sanitizes a string input and limits its length.
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <returns>The sanitized and truncated string.</returns>
        string SanitizeString(string input, int maxLength);
    }
}