using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Queries.PersonaManagement
{
    public class GetPersonaQuery : IRequest<PersonaDto>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}