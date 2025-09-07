using System;

namespace FunnyActivities.Domain.ValueObjects
{
    public class Password : IEquatable<Password>
    {
        public string Value { get; }

        public Password(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters");
            Value = value;
        }

        public bool Equals(Password other) => other != null && Value == other.Value;

        public override bool Equals(object obj) => Equals(obj as Password);

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Password left, Password right) => Equals(left, right);

        public static bool operator !=(Password left, Password right) => !Equals(left, right);
    }
}