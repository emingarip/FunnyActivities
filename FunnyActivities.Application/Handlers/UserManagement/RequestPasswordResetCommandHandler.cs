using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers
{
    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Unit>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

        public RequestPasswordResetCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, IMediator mediator, ILogger<RequestPasswordResetCommandHandler> logger)
        {
            _userRepository = userRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                // For security, don't reveal if email exists
                return Unit.Value;
            }

            var token = Guid.NewGuid().ToString(); // In real app, use secure random
            var expiry = DateTime.UtcNow.AddHours(1);

            var frontendUrl = request.FrontendUrl?.TrimEnd('/')
                ?? Environment.GetEnvironmentVariable("FRONTEND_URL")?.TrimEnd('/')
                ?? "https://makethen.com";
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

            user.SetResetToken(token, expiry);
            await _userRepository.UpdateAsync(user);

            // Send password reset email
            try
            {
                await _mediator.Send(new SendPasswordResetEmailCommand
                {
                    Email = user.Email,
                    ResetToken = token,
                    ResetLink = resetLink
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send password reset email to {Email}", user.Email);
                // Do not fail the request to avoid leaking user existence; token is still stored.
            }

            return Unit.Value;
        }
    }
}
