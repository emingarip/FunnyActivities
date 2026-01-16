namespace FunnyActivities.Application.PromptTemplates
{
    public class PromptTemplateSeed
    {
        public string Key { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Locale { get; init; } = "en-US";
        public string Content { get; init; } = string.Empty;
        public string? OutputFormatHint { get; init; }
        public string? ProviderHint { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
    }
}
