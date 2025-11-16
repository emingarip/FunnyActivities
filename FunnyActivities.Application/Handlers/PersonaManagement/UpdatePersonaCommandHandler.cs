using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class UpdatePersonaCommandHandler : IRequestHandler<UpdatePersonaCommand, PersonaDto>
    {
        private readonly IPersonaRepository _personaRepository;

        public UpdatePersonaCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<PersonaDto> Handle(UpdatePersonaCommand request, CancellationToken cancellationToken)
        {
            // Log incoming request for debugging
            Console.WriteLine($"[DEBUG] UpdatePersonaCommand received: Id={request.Id}, Age={request.Age}, Gender={request.Gender} (type: {request.Gender?.GetType().Name}), Nationality={request.Nationality}, Biography={request.Biography}");

            var persona = await _personaRepository.GetByIdAsync(request.Id);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            if (persona.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this persona.");
            }

            // Check if the new name conflicts with existing personas for this user
            if (persona.Name != request.Name && await _personaRepository.ExistsByNameAndUserIdAsync(request.Name, request.UserId))
            {
                throw new InvalidOperationException("A persona with this name already exists for the user.");
            }

            // Log persona state before update
            Console.WriteLine($"[DEBUG] Persona before update: Age={persona.Age}, Gender={persona.Gender}, Nationality={persona.Nationality}, Biography={persona.Biography}");

            persona.UpdateDetails(request.Name, request.Description, request.AvatarImageUrl, request.Age, request.Gender, request.Nationality, request.Biography);

            // Log persona state after update
            Console.WriteLine($"[DEBUG] Persona after update: Age={persona.Age}, Gender={persona.Gender}, Nationality={persona.Nationality}, Biography={persona.Biography}");

            await _personaRepository.UpdateAsync(persona);

            var result = MapToDto(persona);
            // Log result
            Console.WriteLine($"[DEBUG] UpdatePersonaCommand result: Age={result.Age}, Gender={result.Gender}, Nationality={result.Nationality}, Biography={result.Biography}");

            return result;
        }

        private PersonaDto MapToDto(Domain.Entities.Persona persona)
        {
            return new PersonaDto
            {
                Id = persona.Id,
                UserId = persona.UserId,
                Name = persona.Name,
                Description = persona.Description,
                AvatarImageUrl = persona.AvatarImageUrl,
                Age = persona.Age,
                Gender = persona.Gender,
                Nationality = persona.Nationality,
                Biography = persona.Biography,
                Characteristics = persona.Characteristics.Select(c => new PersonaCharacteristicDto
                {
                    Id = c.Id,
                    PersonaId = c.PersonaId,
                    Name = c.Name,
                    Value = c.Value,
                    Type = c.Type,
                    Order = c.Order,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList(),
                ActivityAssociations = persona.ActivityAssociations.Select(a => new PersonaActivityAssociationDto
                {
                    Id = a.Id,
                    PersonaId = a.PersonaId,
                    ActivityId = a.ActivityId,
                    ActivityName = a.Activity?.Name ?? string.Empty,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList(),
                CreatedAt = persona.CreatedAt,
                UpdatedAt = persona.UpdatedAt
            };
        }
    }
}