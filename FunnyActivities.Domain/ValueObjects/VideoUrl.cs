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
        /// <param name="url">The video URL string.</param>
        /// <returns>A new video URL instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the URL is invalid.</exception>
        public static VideoUrl Create(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Video URL cannot be null or empty.", nameof(url));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Invalid video URL format.", nameof(url));

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