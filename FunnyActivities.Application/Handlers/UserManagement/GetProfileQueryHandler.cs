using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Queries.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Handlers
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserDto>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;

        public GetProfileQueryHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                return null;

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