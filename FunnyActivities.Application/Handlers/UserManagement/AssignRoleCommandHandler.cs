using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FunnyActivities.Application.Handlers
{
    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, bool>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public AssignRoleCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<bool> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            // Get the user to assign role to
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return false;
            }

            // Get the user who is assigning the role
            var assignedByUser = await _userRepository.GetByIdAsync(request.AssignedByUserId);
            if (assignedByUser == null || assignedByUser.Role != UserRole.Admin)
            {
                return false; // Only admins can assign roles
            }

            var oldRole = user.Role;

            // Assign the new role
            user.AssignRole(request.NewRole);

            // Update the user
            await _userRepository.UpdateAsync(user);

            // Log the role change
            var auditLog = new AuditLog(
                userId: request.AssignedByUserId,
                action: "RoleChanged",
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                details: $"User {user.Email} role changed from {oldRole} to {request.NewRole} by {assignedByUser.Email}"
            );

            await _auditLogRepository.AddAsync(auditLog);

            return true;
        }
    }
}