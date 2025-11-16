using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.DTOs.PersonaManagement
{
    public class PersonaDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? AvatarImageUrl { get; set; }
        public int? Age { get; set; }
        public Gender? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? Biography { get; set; }
        public List<PersonaCharacteristicDto> Characteristics { get; set; } = new();
        public List<PersonaActivityAssociationDto> ActivityAssociations { get; set; } = new();
        public List<PersonaImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
