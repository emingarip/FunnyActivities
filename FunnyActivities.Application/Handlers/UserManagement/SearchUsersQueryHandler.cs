using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Queries.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Handlers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, SearchUsersResponse>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;

        public SearchUsersQueryHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<SearchUsersResponse> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var (users, totalCount) = await _userRepository.SearchAsync(
                request.SearchTerm,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfileImageUrl = u.ProfileImageUrl,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            });

            return new SearchUsersResponse
            {
                Users = userDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}