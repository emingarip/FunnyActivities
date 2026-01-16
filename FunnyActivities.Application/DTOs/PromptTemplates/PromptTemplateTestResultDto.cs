namespace FunnyActivities.Application.DTOs.PromptTemplates
{
    public class PromptTemplateTestResultDto
    {
        public PromptTemplateDto? Template { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public double Duration { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
