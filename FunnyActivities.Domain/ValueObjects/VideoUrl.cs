using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.ValueObjects
{
    /// <summary>
    /// Represents a video URL value object.
    /// </summary>
    public class VideoUrl
    {
        /// <summary>
        /// Gets the video URL value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoUrl"/> class.
        /// </summary>
        /// <param name="value">The video URL value.</param>
        private VideoUrl(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new video URL instance.
        /// </summary>
        /// <param name="url">The video URL string or MinIO object key.</param>
        /// <returns>A new video URL instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the URL is invalid.</exception>
        public static VideoUrl Create(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Video URL cannot be null or empty.", nameof(url));

            // Allow MinIO object keys (they don't have URL format)
            // or valid HTTP/HTTPS URLs with more flexible validation
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            {
                // If it's not a valid URL, treat it as a MinIO object key
                // Allow any non-empty string for object keys (MinIO handles validation)
                // But ensure it's not just whitespace or special characters
                if (url.Trim().Length == 0 || url.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r'))
                {
                    throw new ArgumentException("Video URL contains invalid characters.", nameof(url));
                }
            }
            else
            {
                // For valid URIs, allow more schemes beyond just HTTP/HTTPS
                // This includes data URIs, blob URIs, and other valid URI schemes
                var allowedSchemes = new[] {
                    Uri.UriSchemeHttp,
                    Uri.UriSchemeHttps,
                    "data",    // Data URIs for embedded content
                    "blob",    // Blob URIs for local content
                    "file",    // File URIs for local files
                    "rtmp",    // RTMP streams
                    "rtsp",    // RTSP streams
                    "mms"      // MMS streams
                };

                if (!allowedSchemes.Contains(uriResult.Scheme.ToLower()))
                {
                    throw new ArgumentException($"Video URL scheme '{uriResult.Scheme}' is not supported. Supported schemes are: {string.Join(", ", allowedSchemes)}.", nameof(url));
                }
            }

            return new VideoUrl(url);
        }

        /// <summary>
        /// Returns the string representation of the video URL.
        /// </summary>
        /// <returns>The video URL value.</returns>
        public override string ToString() => Value;

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is VideoUrl other)
                return Value == other.Value;
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => Value.GetHashCode();
    }
}