using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class UpdatePersonaActivityAssociationCommandHandler : IRequestHandler<UpdatePersonaActivityAssociationCommand, PersonaActivityAssociationDto>
    {
        private readonly IPersonaRepository _personaRepository;

        public UpdatePersonaActivityAssociationCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<PersonaActivityAssociationDto> Handle(UpdatePersonaActivityAssociationCommand request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.Id);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            var association = persona.ActivityAssociations.FirstOrDefault(a => a.Id == request.Id);
            if (association == null)
            {
                throw new KeyNotFoundException("Association not found.");
            }


            await _personaRepository.UpdateAsync(persona);

            return MapToDto(association);
        }

        private PersonaActivityAssociationDto MapToDto(Domain.Entities.PersonaActivityAssociation association)
        {
            return new PersonaActivityAssociationDto
            {
                Id = association.Id,
                PersonaId = association.PersonaId,
                ActivityId = association.ActivityId,
                ActivityName = association.Activity?.Name ?? string.Empty,
                CreatedAt = association.CreatedAt,
                UpdatedAt = association.UpdatedAt
            };
        }
    }
}