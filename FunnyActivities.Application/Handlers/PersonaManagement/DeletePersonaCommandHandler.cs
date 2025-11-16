using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class DeletePersonaCommandHandler : IRequestHandler<DeletePersonaCommand, Unit>
    {
        private readonly IPersonaRepository _personaRepository;

        public DeletePersonaCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<Unit> Handle(DeletePersonaCommand request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.Id);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            if (persona.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this persona.");
            }

            await _personaRepository.DeleteAsync(persona);

            return Unit.Value;
        }
    }
}