namespace FunnyActivities.Application.DTOs.PersonaManagement
{
    public class PersonaCharacteristicDto
    {
        public Guid Id { get; set; }
        public Guid PersonaId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string? Type { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}