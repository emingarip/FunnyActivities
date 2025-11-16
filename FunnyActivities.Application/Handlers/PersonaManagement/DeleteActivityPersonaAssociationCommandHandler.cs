using MediatR;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class DeleteActivityPersonaAssociationCommandHandler : IRequestHandler<DeleteActivityPersonaAssociationCommand>
    {
        private readonly IPersonaRepository _personaRepository;

        public DeleteActivityPersonaAssociationCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task Handle(DeleteActivityPersonaAssociationCommand request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.Id);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            var association = persona.ActivityAssociations.FirstOrDefault(a => a.Id == request.Id);
            if (association == null)
            {
                throw new KeyNotFoundException("Activity association not found.");
            }

            persona.RemoveActivityAssociation(association);
            await _personaRepository.UpdateAsync(persona);
        }
    }
}