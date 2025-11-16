using MediatR;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class DeletePersonaActivityAssociationCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}