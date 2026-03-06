using MediatR;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.Application.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly UserService _userService;

        public ChangePasswordCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, UserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!_userService.VerifyPassword(user.PasswordHash, request.CurrentPassword))
            {
                throw new ArgumentException("Current password is incorrect");
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                throw new ArgumentException("New password must be different from the current password");
            }

            var newPassword = new Password(request.NewPassword);
            var hashedPassword = _userService.HashPassword(newPassword);
            user.SetPasswordHash(hashedPassword);

            await _userRepository.UpdateAsync(user);

            return Unit.Value;
        }
    }
}
