using MediatR;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class DeletePersonaCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}