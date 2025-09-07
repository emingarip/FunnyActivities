using System;

namespace FunnyActivities.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? ProfileImageUrl { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string? ResetToken { get; private set; }
        public DateTime? ResetTokenExpiry { get; private set; }

        public User(Guid id, string email, string passwordHash, string firstName, string lastName, UserRole role = UserRole.User)
        {
            Id = id;
            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Private constructor for EF or ORM
        private User() { }

        public void UpdateProfile(string firstName, string lastName, string profileImageUrl)
        {
            FirstName = firstName;
            LastName = lastName;
            ProfileImageUrl = profileImageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetResetToken(string token, DateTime expiry)
        {
            ResetToken = token;
            ResetTokenExpiry = expiry;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearResetToken()
        {
            ResetToken = null;
            ResetTokenExpiry = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignRole(UserRole newRole)
        {
            Role = newRole;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}