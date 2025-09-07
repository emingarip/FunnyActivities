using MediatR;

namespace FunnyActivities.Application.Commands.NotificationSystem
{
    public class SendRegistrationConfirmationEmailCommand : IRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
    }
}