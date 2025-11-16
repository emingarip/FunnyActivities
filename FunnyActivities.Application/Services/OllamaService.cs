using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Services
{
    public class OllamaService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService> _logger;
        private readonly OllamaSettings _settings;

        public OllamaService(HttpClient httpClient, ILogger<OllamaService> logger, IOptions<OllamaSettings> settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<string> GenerateContentAsync(string prompt, string model = "llama2")
        {
            try
            {
                var request = new OllamaRequest
                {
                    Model = model,
                    Prompt = prompt,
                    Stream = false
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/api/generate", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ollama API request failed with status code: {StatusCode}", response.StatusCode);
                    throw new Exception($"Ollama API request failed: {response.StatusCode}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson);

                return ollamaResponse?.Response ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content with Ollama");
                throw;
            }
        }

        public async Task<string> GeneratePersonaContentAsync(string personaDescription, string activityDescription, string model = "llama2")
        {
            var prompt = $"Based on this persona: {personaDescription}\n\nGenerate engaging content for this activity: {activityDescription}\n\nMake it personalized and fun.";
            return await GenerateContentAsync(prompt, model);
        }

        public async Task<string> GenerateStoryAsync(string personaDescription, string activityDescription, string model = "llama2")
        {
            var prompt = $"Create an engaging story based on this persona: {personaDescription}\n\nThe story should revolve around this activity: {activityDescription}\n\nMake it narrative-driven, immersive, and personalized to the persona's characteristics. Include a beginning, middle, and end.";
            return await GenerateContentAsync(prompt, model);
        }

        public async Task<string> GenerateNarrativeAsync(string personaDescription, string activityDescription, string model = "llama2")
        {
            var prompt = $"Write a compelling narrative from the perspective of this persona: {personaDescription}\n\nThe narrative should describe their experience with this activity: {activityDescription}\n\nUse first-person perspective and make it vivid and engaging.";
            return await GenerateContentAsync(prompt, model);
        }

        public async Task<string> GenerateTipsAsync(string personaDescription, string activityDescription, string model = "llama2")
        {
            var prompt = $"Based on this persona: {personaDescription}\n\nProvide personalized tips and advice for successfully completing this activity: {activityDescription}\n\nTailor the tips to the persona's characteristics and make them practical and actionable.";
            return await GenerateContentAsync(prompt, model);
        }

        public async Task<bool> ValidateConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Ollama connection");
                return false;
            }
        }

        public async Task<IEnumerable<string>> ListAvailableModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/api/tags");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to list models with status code: {StatusCode}", response.StatusCode);
                    return new List<string>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(responseJson);

                return modelsResponse?.Models?.Select(m => m.Name) ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing available models");
                return new List<string>();
            }
        }

        private class OllamaRequest
        {
            public string Model { get; set; }
            public string Prompt { get; set; }
            public bool Stream { get; set; }
        }

        private class OllamaResponse
        {
            public string Response { get; set; }
        }

        private class ModelsResponse
        {
            public List<ModelInfo> Models { get; set; }
        }

        private class ModelInfo
        {
            public string Name { get; set; }
        }
    }

    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
    }
}