using FunnyActivities.Domain.Entities;
using MediatR;

namespace FunnyActivities.Application.Queries.UserManagement
{
    public class GetUserRoleQuery : IRequest<UserRole?>
    {
        public Guid UserId { get; set; }

        public GetUserRoleQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}