using System;

namespace FunnyActivities.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Role(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Private constructor for EF or ORM
        private Role() { }

        public void UpdateDescription(string description)
        {
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum UserRole
    {
        Admin = 1,
        User = 2,
        Viewer = 3
    }
}