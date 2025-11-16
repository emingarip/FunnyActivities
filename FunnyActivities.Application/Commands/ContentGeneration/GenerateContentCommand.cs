using MediatR;

namespace FunnyActivities.Application.Commands.ContentGeneration
{
    public enum ContentType
    {
        General,
        Story,
        Narrative,
        Tips
    }

    public class GenerateContentCommand : IRequest<string>
    {
        public Guid UserId { get; set; }
        public Guid PersonaId { get; set; }
        public Guid ActivityId { get; set; }
        public string? CustomPrompt { get; set; }
        public string Model { get; set; } = "llama2";
        public ContentType ContentType { get; set; } = ContentType.General;
    }
}