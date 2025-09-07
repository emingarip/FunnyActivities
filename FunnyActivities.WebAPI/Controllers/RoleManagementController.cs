using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.Queries.UserManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.WebAPI.Controllers.Base;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FunnyActivities.WebAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    public class RoleManagementController : BaseController
    {
        private readonly IMediator _mediator;

        public RoleManagementController(IMediator mediator, ILogger<RoleManagementController> logger)
            : base(logger)
        {
            _mediator = mediator;
        }

        [HttpPost("assign")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            // User ID is automatically validated and available through BaseController
            var command = new AssignRoleCommand(
                userId: request.UserId,
                newRole: request.Role,
                assignedByUserId: CurrentUserId,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                userAgent: HttpContext.Request.Headers["User-Agent"].ToString()
            );

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to assign role. User not found or insufficient permissions." });
            }

            return Ok(new { message = "Role assigned successfully." });
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> GetUserRole(Guid userId)
        {
            var query = new GetUserRoleQuery(userId);
            var role = await _mediator.Send(query);

            if (role == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { userId, role = role.ToString() });
        }
    }

    public class AssignRoleRequest
    {
        public Guid UserId { get; set; }
        public UserRole Role { get; set; }
    }
}