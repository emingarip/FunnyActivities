using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class CreatePersonaActivityAssociationCommand : IRequest<PersonaActivityAssociationDto>
    {
        public Guid PersonaId { get; set; }
        public Guid ActivityId { get; set; }
    }
}