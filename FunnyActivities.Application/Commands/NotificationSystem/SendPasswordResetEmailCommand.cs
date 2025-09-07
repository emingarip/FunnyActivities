using MediatR;

namespace FunnyActivities.Application.Commands.NotificationSystem
{
    public class SendPasswordResetEmailCommand : IRequest
    {
        public string Email { get; set; }
        public string ResetToken { get; set; }
    }
}