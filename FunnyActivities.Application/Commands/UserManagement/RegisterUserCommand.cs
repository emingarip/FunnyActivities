using MediatR;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class RegisterUserCommand : IRequest<Unit>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
