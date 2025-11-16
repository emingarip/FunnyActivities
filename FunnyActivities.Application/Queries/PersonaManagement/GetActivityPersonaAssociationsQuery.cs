using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Queries.PersonaManagement
{
    public class GetActivityPersonaAssociationsQuery : IRequest<List<PersonaActivityAssociationDto>>
    {
        public Guid ActivityId { get; set; }
    }
}