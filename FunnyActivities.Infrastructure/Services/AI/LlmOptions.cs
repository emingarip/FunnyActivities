using System;
using FunnyActivities.Application.AI;

namespace FunnyActivities.Infrastructure.Services.AI
{
    public class LlmOptions
    {
        public LlmProvider DefaultProvider { get; set; } = LlmProvider.Ollama;
        public string DefaultModel { get; set; } = "llama2";
        public int ModelCacheSeconds { get; set; } = 300;
        public OllamaOptions Ollama { get; set; } = new();
        public OpenAiOptions OpenAI { get; set; } = new();
    }

    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string HealthCheckModel { get; set; } = "llama2";
        public string[] PreferredModels { get; set; } = Array.Empty<string>();
    }

    public class OpenAiOptions
    {
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public string ApiKey { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gpt-4o-mini";
        public string[] AllowedModels { get; set; } = Array.Empty<string>();
        public string DefaultSystemPrompt { get; set; } = "You are a helpful assistant.";
    }
}
