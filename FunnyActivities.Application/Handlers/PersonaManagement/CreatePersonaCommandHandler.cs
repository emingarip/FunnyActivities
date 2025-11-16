using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class CreatePersonaCommandHandler : IRequestHandler<CreatePersonaCommand, PersonaDto>
    {
        private readonly IPersonaRepository _personaRepository;

        public CreatePersonaCommandHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<PersonaDto> Handle(CreatePersonaCommand request, CancellationToken cancellationToken)
        {
            // Check if persona name already exists for this user
            if (await _personaRepository.ExistsByNameAndUserIdAsync(request.Name, request.UserId))
            {
                throw new InvalidOperationException("A persona with this name already exists for the user.");
            }

            var persona = Persona.Create(request.UserId, request.Name, request.Description, request.AvatarImageUrl, request.Age, request.Gender, request.Nationality, request.Biography);

            // Add characteristics if provided
            if (request.Characteristics != null)
            {
                foreach (var characteristicDto in request.Characteristics)
                {
                    var characteristic = PersonaCharacteristic.Create(
                        persona.Id,
                        characteristicDto.Name,
                        characteristicDto.Value,
                        characteristicDto.Type,
                        characteristicDto.Order);

                    persona.AddCharacteristic(characteristic);
                }
            }

            await _personaRepository.AddAsync(persona);

            return MapToDto(persona);
        }

        private PersonaDto MapToDto(Persona persona)
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
                Images = persona.Images.Select(i => new PersonaImageDto
                {
                    Id = i.Id,
                    PersonaId = i.PersonaId,
                    FileName = i.FileName,
                    OriginalFileName = i.OriginalFileName,
                    ContentType = i.ContentType,
                    FileSize = i.FileSize,
                    BucketName = i.BucketName,
                    ObjectKey = i.ObjectKey,
                    PreSignedUrl = i.PreSignedUrl,
                    ImageType = i.ImageType,
                    UploadedAt = i.UploadedAt
                }).ToList(),
                CreatedAt = persona.CreatedAt,
                UpdatedAt = persona.UpdatedAt
            };
        }
    }
}
