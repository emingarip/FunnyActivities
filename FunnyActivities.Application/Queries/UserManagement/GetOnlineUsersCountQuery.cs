using MediatR;

namespace FunnyActivities.Application.Queries.UserManagement;

public class GetOnlineUsersCountQuery : IRequest<int>
{
    public TimeSpan OnlineThreshold { get; set; } = TimeSpan.FromMinutes(30);
}