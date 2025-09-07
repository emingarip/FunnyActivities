using MediatR;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Queries.UserManagement
{
    public class GetProfileQuery : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
    }
}