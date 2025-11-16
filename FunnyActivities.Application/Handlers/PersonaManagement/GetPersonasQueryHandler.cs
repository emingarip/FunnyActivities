using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Queries.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class GetPersonasQueryHandler : IRequestHandler<GetPersonasQuery, PagedResult<PersonaDto>>
    {
        private readonly IPersonaRepository _personaRepository;

        public GetPersonasQueryHandler(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<PagedResult<PersonaDto>> Handle(GetPersonasQuery request, CancellationToken cancellationToken)
        {
            var (personas, totalCount) = await _personaRepository.GetByUserIdPagedAsync(
                request.UserId,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder);

            var personaDtos = personas.Select(MapToDto).ToList();

            return new PagedResult<PersonaDto>(personaDtos, request.Page, request.PageSize, totalCount);
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
