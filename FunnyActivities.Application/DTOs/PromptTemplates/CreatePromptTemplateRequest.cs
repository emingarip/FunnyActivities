namespace FunnyActivities.Application.DTOs.PromptTemplates
{
    public class CreatePromptTemplateRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Locale { get; set; } = "en-US";
        public string? ProviderHint { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? OutputFormatHint { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}
