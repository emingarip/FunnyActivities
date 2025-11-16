using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class CreatePersonaActivityAssociationCommandHandler : IRequestHandler<CreatePersonaActivityAssociationCommand, PersonaActivityAssociationDto>
    {
        private readonly IPersonaRepository _personaRepository;

        public CreatePersonaActivityAssociationCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<PersonaActivityAssociationDto> Handle(CreatePersonaActivityAssociationCommand request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.PersonaId);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            // Check if association already exists
            var existingAssociation = persona.ActivityAssociations.FirstOrDefault(a => a.ActivityId == request.ActivityId);
            if (existingAssociation != null)
            {
                throw new InvalidOperationException("An association with this activity already exists for the persona.");
            }

            var association = PersonaActivityAssociation.Create(request.PersonaId, request.ActivityId);
            persona.AddActivityAssociation(association);

            await _personaRepository.UpdateAsync(persona);

            return MapToDto(association);
        }

        private PersonaActivityAssociationDto MapToDto(PersonaActivityAssociation association)
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