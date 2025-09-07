using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.UserManagement;
using FunnyActivities.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FunnyActivities.Application.Handlers
{
    public class GetUserRoleQueryHandler : IRequestHandler<GetUserRoleQuery, UserRole?>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;

        public GetUserRoleQueryHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserRole?> Handle(GetUserRoleQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            return user?.Role;
        }
    }
}