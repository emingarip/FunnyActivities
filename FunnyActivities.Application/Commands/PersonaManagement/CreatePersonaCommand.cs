using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class CreatePersonaCommand : IRequest<PersonaDto>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? AvatarImageUrl { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
        public List<PersonaCharacteristicDto>? Characteristics { get; set; }
    }
}