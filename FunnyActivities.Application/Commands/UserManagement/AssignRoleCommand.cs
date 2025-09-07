using FunnyActivities.Domain.Entities;
using MediatR;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class AssignRoleCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public UserRole NewRole { get; set; }
        public Guid AssignedByUserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        public AssignRoleCommand(Guid userId, UserRole newRole, Guid assignedByUserId, string ipAddress, string userAgent)
        {
            UserId = userId;
            NewRole = newRole;
            AssignedByUserId = assignedByUserId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}