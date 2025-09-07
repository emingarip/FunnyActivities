using MediatR;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class ResetPasswordCommand : IRequest<Unit>
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}