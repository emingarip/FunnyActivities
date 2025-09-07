using MediatR;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class UpdateProfileCommand : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfileImageUrl { get; set; } // Nullable - only set when image is uploaded
    }
}