using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Handlers
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;

        public UpdateProfileCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.UpdateProfile(request.FirstName, request.LastName, request.ProfileImageUrl);

            await _userRepository.UpdateAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}