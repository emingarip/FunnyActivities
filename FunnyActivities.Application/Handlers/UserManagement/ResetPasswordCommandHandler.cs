using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;

namespace FunnyActivities.Application.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly UserService _userService;

        public ResetPasswordCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, UserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByResetTokenAsync(request.Token);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid or expired token");
            }

            var hashedPassword = _userService.HashPassword(new FunnyActivities.Domain.ValueObjects.Password(request.NewPassword));
            user.SetPasswordHash(hashedPassword);
            user.ClearResetToken();

            await _userRepository.UpdateAsync(user);

            return Unit.Value;
        }
    }
}