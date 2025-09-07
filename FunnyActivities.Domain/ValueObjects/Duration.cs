using System;

namespace FunnyActivities.Domain.ValueObjects
{
    /// <summary>
    /// Represents a duration value object.
    /// </summary>
    public class Duration
    {
        /// <summary>
        /// Gets the duration value in TimeSpan.
        /// </summary>
        public TimeSpan Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Duration"/> class.
        /// </summary>
        /// <param name="value">The duration value.</param>
        private Duration(TimeSpan value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new duration instance.
        /// </summary>
        /// <param name="hours">The hours component.</param>
        /// <param name="minutes">The minutes component.</param>
        /// <param name="seconds">The seconds component.</param>
        /// <returns>A new duration instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the duration is invalid.</exception>
        public static Duration Create(int hours, int minutes, int seconds)
        {
            if (hours < 0 || minutes < 0 || seconds < 0)
                throw new ArgumentException("Duration components cannot be negative.");

            if (minutes >= 60 || seconds >= 60)
                throw new ArgumentException("Minutes and seconds must be less than 60.");

            var timeSpan = new TimeSpan(hours, minutes, seconds);
            if (timeSpan.TotalSeconds <= 0)
                throw new ArgumentException("Duration must be greater than zero.");

            return new Duration(timeSpan);
        }

        /// <summary>
        /// Creates a new duration instance from TimeSpan.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan value.</param>
        /// <returns>A new duration instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the TimeSpan is invalid.</exception>
        public static Duration Create(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds <= 0)
                throw new ArgumentException("Duration must be greater than zero.", nameof(timeSpan));

            return new Duration(timeSpan);
        }

        /// <summary>
        /// Returns the string representation of the duration.
        /// </summary>
        /// <returns>The duration in HH:MM:SS format.</returns>
        public override string ToString() => Value.ToString(@"hh\:mm\:ss");

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Duration other)
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