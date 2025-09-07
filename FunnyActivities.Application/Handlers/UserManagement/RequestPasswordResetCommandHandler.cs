using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Handlers
{
    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Unit>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public RequestPasswordResetCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, IMediator mediator)
        {
            _userRepository = userRepository;
            _mediator = mediator;
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

            user.SetResetToken(token, expiry);
            await _userRepository.UpdateAsync(user);

            // Send password reset email
            await _mediator.Send(new SendPasswordResetEmailCommand
            {
                Email = user.Email,
                ResetToken = token
            });

            return Unit.Value;
        }
    }
}