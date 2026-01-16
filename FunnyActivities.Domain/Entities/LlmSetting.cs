using System;

namespace FunnyActivities.Domain.Entities
{
    public class LlmSetting
    {
        public int Id { get; set; } = 1;
        public string DefaultProvider { get; set; } = "Ollama";
        public string DefaultModel { get; set; } = "llama3";
        public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
        public string OllamaHealthCheckModel { get; set; } = "llama3";
        public string OllamaPreferredModelsJson { get; set; } = "[]";
        public string OpenAiBaseUrl { get; set; } = "https://api.openai.com/v1";
        public string OpenAiDefaultModel { get; set; } = "gpt-4o-mini";
        public string OpenAiAllowedModelsJson { get; set; } = "[]";
        public string OpenAiOrganizationId { get; set; } = string.Empty;
        public string OpenAiApiKey { get; set; } = string.Empty;
        public int ModelCacheSeconds { get; set; } = 300;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
    }
}
