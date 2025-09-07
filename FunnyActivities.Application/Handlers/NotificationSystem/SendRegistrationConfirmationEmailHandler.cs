using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers
{
    public class SendRegistrationConfirmationEmailHandler : IRequestHandler<SendRegistrationConfirmationEmailCommand>
    {
        private readonly IEmailService _emailService;

        public SendRegistrationConfirmationEmailHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(SendRegistrationConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            var subject = "Welcome to FunnyActivities!";
            var body = $"Hi {request.FirstName},\n\nWelcome to FunnyActivities! Your account has been successfully created.\n\nBest regards,\nFunnyActivities Team";
            await _emailService.SendEmailAsync(request.Email, subject, body);
        }
    }
}