using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.ValueObjects;
using FunnyActivities.Domain.Services;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Application.Handlers.UserManagement
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly UserService _userService;
        private readonly IMediator _mediator;

        public RegisterUserCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, UserService userService, IMediator mediator)
        {
            _userRepository = userRepository;
            _userService = userService;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var email = new Email(request.Email);
            var password = new Password(request.Password);

            if (!_userService.IsValidEmail(email))
                throw new InvalidOperationException("Invalid email");

            if (await _userRepository.ExistsByEmailAsync(request.Email))
                throw new InvalidOperationException("User already exists");

            var hashedPassword = _userService.HashPassword(password);

            var user = new User(Guid.NewGuid(), request.Email, hashedPassword, request.FirstName, request.LastName, request.Role);

            await _userRepository.AddAsync(user);

            // Send registration confirmation email
            await _mediator.Send(new SendRegistrationConfirmationEmailCommand
            {
                Email = request.Email,
                FirstName = request.FirstName
            });

            // Domain event would be raised here, but since no event handler, just create it
            var userRegisteredEvent = new UserRegisteredEvent(user);
            // In real app, publish event

            return Unit.Value;
        }
    }
}