using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.DTOs.UserManagement
{
    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; } = UserRole.User; // Default to User role
    }
}