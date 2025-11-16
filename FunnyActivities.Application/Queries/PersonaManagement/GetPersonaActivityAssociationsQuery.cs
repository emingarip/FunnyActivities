using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Queries.PersonaManagement
{
    public class GetPersonaActivityAssociationsQuery : IRequest<List<PersonaActivityAssociationDto>>
    {
        public Guid PersonaId { get; set; }
    }
}