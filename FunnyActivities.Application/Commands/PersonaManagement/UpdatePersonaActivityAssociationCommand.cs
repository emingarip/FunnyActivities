using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class UpdatePersonaActivityAssociationCommand : IRequest<PersonaActivityAssociationDto>
    {
        public Guid Id { get; set; }
    }
}