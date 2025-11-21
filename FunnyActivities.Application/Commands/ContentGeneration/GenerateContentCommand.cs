using FunnyActivities.Application.AI;
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
        public string? Model { get; set; }
        public LlmProvider Provider { get; set; } = LlmProvider.Ollama;
        public float? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public string? SystemPrompt { get; set; }
        public ContentType ContentType { get; set; } = ContentType.General;
    }
}
