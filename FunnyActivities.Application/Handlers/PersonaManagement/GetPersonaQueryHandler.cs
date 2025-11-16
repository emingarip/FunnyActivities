using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class GetPersonaQueryHandler : IRequestHandler<GetPersonaQuery, PersonaDto>
    {
        private readonly IPersonaRepository _personaRepository;

        public GetPersonaQueryHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<PersonaDto> Handle(GetPersonaQuery request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.Id);

            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            if (persona.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this persona.");
            }

            return MapToDto(persona);
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
