namespace FunnyActivities.Application.DTOs.Settings
{
    public class UpdateLlmSettingsRequest
    {
        public string DefaultProvider { get; set; } = "Ollama";
        public string DefaultModel { get; set; } = "llama3";
        public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
        public string OllamaHealthCheckModel { get; set; } = "llama3";
        public string[] OllamaPreferredModels { get; set; } = [];
        public string OpenAiBaseUrl { get; set; } = "https://api.openai.com/v1";
        public string OpenAiDefaultModel { get; set; } = "gpt-4o-mini";
        public string[] OpenAiAllowedModels { get; set; } = [];
        public string OpenAiOrganizationId { get; set; } = string.Empty;
        public string? OpenAiApiKey { get; set; }
        public int ModelCacheSeconds { get; set; } = 300;
    }
}
