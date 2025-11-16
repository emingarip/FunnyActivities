namespace FunnyActivities.Application.DTOs.PersonaManagement
{
    public class PersonaActivityAssociationDto
    {
        public Guid Id { get; set; }
        public Guid PersonaId { get; set; }
        public PersonaDto Persona { get; set; }
        public Guid ActivityId { get; set; }
        public string ActivityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}