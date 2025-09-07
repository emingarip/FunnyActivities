using System;

namespace FunnyActivities.Domain.ValueObjects
{
    public class Email : IEquatable<Email>
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty");
            // Add email validation logic here
            Value = value;
        }

        public bool Equals(Email other) => other != null && Value == other.Value;

        public override bool Equals(object obj) => Equals(obj as Email);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Email left, Email right) => Equals(left, right);

        public static bool operator !=(Email left, Email right) => !Equals(left, right);
    }
}