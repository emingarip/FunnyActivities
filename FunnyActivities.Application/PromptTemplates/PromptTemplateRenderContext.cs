using System.Collections.Generic;

namespace FunnyActivities.Application.PromptTemplates
{
    public class PromptTemplateRenderContext
    {
        public string PersonaDescription { get; init; } = string.Empty;
        public string ActivityDescription { get; init; } = string.Empty;
        public string? PersonaName { get; init; }
        public string? ActivityName { get; init; }
        public string? CustomPrompt { get; init; }
        public string? SystemPrompt { get; init; }
        public string Locale { get; init; } = "en-US";
        public string Scenario { get; init; } = "general";
        public IDictionary<string, string>? AdditionalVariables { get; init; }
    }
}
