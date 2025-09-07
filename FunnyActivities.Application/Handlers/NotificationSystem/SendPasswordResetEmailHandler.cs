using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers
{
    public class SendPasswordResetEmailHandler : IRequestHandler<SendPasswordResetEmailCommand>
    {
        private readonly IEmailService _emailService;

        public SendPasswordResetEmailHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
        {
            var subject = "Password Reset Request";
            var body = $"Hi,\n\nYou requested a password reset. Use the following token to reset your password: {request.ResetToken}\n\nIf you didn't request this, please ignore this email.\n\nBest regards,\nFunnyActivities Team";
            await _emailService.SendEmailAsync(request.Email, subject, body);
        }
    }
}