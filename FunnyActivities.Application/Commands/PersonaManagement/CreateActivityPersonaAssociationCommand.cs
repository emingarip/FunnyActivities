using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class CreateActivityPersonaAssociationCommand : IRequest<PersonaActivityAssociationDto>
    {
        public Guid ActivityId { get; set; }
        public Guid PersonaId { get; set; }
    }
}