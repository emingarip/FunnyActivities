using MediatR;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class LoginUserCommand : IRequest<LoginResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}