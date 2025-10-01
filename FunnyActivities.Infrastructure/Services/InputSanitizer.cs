using System.Text.RegularExpressions;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure.Services
{
    /// <summary>
    /// Service for sanitizing user input to prevent XSS attacks and improve data quality.
    /// </summary>
    public class InputSanitizer : IInputSanitizer
    {
        /// <summary>
        /// Sanitizes a string input by removing potentially dangerous HTML/script content.
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <returns>The sanitized string.</returns>
        public string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove HTML tags
            var sanitized = Regex.Replace(input, @"<[^>]*>", string.Empty);

            // Remove script tags and their content
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove javascript: protocol
            sanitized = Regex.Replace(sanitized, @"javascript:", string.Empty, RegexOptions.IgnoreCase);

            // Remove vbscript: protocol
            sanitized = Regex.Replace(sanitized, @"vbscript:", string.Empty, RegexOptions.IgnoreCase);

            // Remove data: URLs that might contain scripts
            sanitized = Regex.Replace(sanitized, @"data:text/html[^,]*", string.Empty, RegexOptions.IgnoreCase);

            // Remove event handlers (onClick, onLoad, etc.)
            sanitized = Regex.Replace(sanitized, @"\bon\w+\s*=\s*[""'][^""']*[""']", string.Empty, RegexOptions.IgnoreCase);

            // Trim whitespace
            sanitized = sanitized.Trim();

            return sanitized;
        }

        /// <summary>
        /// Sanitizes a string input and limits its length.
        /// </summary>
        /// <param name="input">The input string to sanitize.</param>
        /// <param name="maxLength">The maximum allowed length.</param>
        /// <returns>The sanitized and truncated string.</returns>
        public string SanitizeString(string input, int maxLength)
        {
            var sanitized = SanitizeString(input);

            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength).TrimEnd();
            }

            return sanitized;
        }
    }
}