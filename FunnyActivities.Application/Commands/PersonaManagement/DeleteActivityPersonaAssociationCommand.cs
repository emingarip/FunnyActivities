using MediatR;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class DeleteActivityPersonaAssociationCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}