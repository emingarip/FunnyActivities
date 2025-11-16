using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class GetPersonaActivityAssociationsQueryHandler : IRequestHandler<GetPersonaActivityAssociationsQuery, List<PersonaActivityAssociationDto>>
    {
        private readonly IPersonaRepository _personaRepository;

        public GetPersonaActivityAssociationsQueryHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<List<PersonaActivityAssociationDto>> Handle(GetPersonaActivityAssociationsQuery request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.PersonaId);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            return persona.ActivityAssociations.Select(a => new PersonaActivityAssociationDto
            {
                Id = a.Id,
                PersonaId = a.PersonaId,
                ActivityId = a.ActivityId,
                ActivityName = a.Activity?.Name ?? string.Empty,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();
        }
    }
}