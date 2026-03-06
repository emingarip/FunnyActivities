using MediatR;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class ChangePasswordCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
