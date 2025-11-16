using MediatR;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class CreateActivityPersonaAssociationCommandHandler : IRequestHandler<CreateActivityPersonaAssociationCommand, PersonaActivityAssociationDto>
    {
        private readonly IPersonaRepository _personaRepository;
        private readonly IPersonaActivityAssociationRepository _associationRepository;

        public CreateActivityPersonaAssociationCommandHandler(
            IPersonaRepository personaRepository,
            IPersonaActivityAssociationRepository associationRepository)
        {
            _personaRepository = personaRepository;
            _associationRepository = associationRepository;
        }

        public async Task<PersonaActivityAssociationDto> Handle(CreateActivityPersonaAssociationCommand request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.PersonaId);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            // Check if association already exists using the dedicated repository
            var existingAssociation = await _associationRepository.ExistsAsync(request.PersonaId, request.ActivityId);
            if (existingAssociation)
            {
                throw new InvalidOperationException("An association with this activity already exists for the persona.");
            }

            var association = PersonaActivityAssociation.Create(request.PersonaId, request.ActivityId);

            // Save the association directly using the dedicated repository to avoid concurrency issues
            await _associationRepository.AddAsync(association);

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