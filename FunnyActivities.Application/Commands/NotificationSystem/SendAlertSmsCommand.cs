using MediatR;

namespace FunnyActivities.Application.Commands.NotificationSystem
{
    public class SendAlertSmsCommand : IRequest
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }
}