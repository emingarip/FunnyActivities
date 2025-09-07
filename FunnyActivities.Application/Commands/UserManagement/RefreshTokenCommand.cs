using MediatR;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class RefreshTokenCommand : IRequest<LoginResponse>
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}