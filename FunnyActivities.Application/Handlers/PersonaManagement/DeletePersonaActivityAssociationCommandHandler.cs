using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class DeletePersonaActivityAssociationCommandHandler : IRequestHandler<DeletePersonaActivityAssociationCommand>
    {
        private readonly IPersonaRepository _personaRepository;

        public DeletePersonaActivityAssociationCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task Handle(DeletePersonaActivityAssociationCommand request, CancellationToken cancellationToken)
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

            persona.RemoveActivityAssociation(association);

            await _personaRepository.UpdateAsync(persona);
        }
    }
}