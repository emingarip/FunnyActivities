using FunnyActivities.Application.DTOs.PromptTemplates;

namespace FunnyActivities.Application.PromptTemplates
{
    public class PromptTemplateRenderResult
    {
        public PromptTemplateDto? Template { get; init; }
        public string Prompt { get; init; } = string.Empty;
        public string? ProviderHint { get; init; }
    }
}
