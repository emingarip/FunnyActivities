namespace FunnyActivities.Application.DTOs.PromptTemplates
{
    public class ClonePromptTemplateRequest
    {
        public string? Key { get; set; }
        public string? Title { get; set; }
        public string? Locale { get; set; }
        public bool? IsActive { get; set; }
    }
}
