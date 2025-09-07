using MediatR;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class RequestPasswordResetCommand : IRequest<Unit>
    {
        public string Email { get; set; }
    }
}