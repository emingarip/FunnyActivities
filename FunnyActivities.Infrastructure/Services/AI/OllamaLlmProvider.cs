using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Infrastructure.Services.AI
{
    public class OllamaLlmProvider : ILlmProvider
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaLlmProvider> _logger;
        private readonly IOptionsMonitor<LlmOptions> _options;

        public OllamaLlmProvider(HttpClient httpClient, ILogger<OllamaLlmProvider> logger, IOptionsMonitor<LlmOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options;
        }

        public LlmProvider ProviderId => LlmProvider.Ollama;

        public async Task<string> GenerateAsync(string prompt, LlmSelection selection, CancellationToken cancellationToken = default)
        {
            var payload = new OllamaRequest
            {
                Model = selection.Model ?? _options.CurrentValue.Ollama.HealthCheckModel ?? "llama2",
                Prompt = prompt,
                Stream = false,
                Options = selection.Temperature.HasValue
                    ? new OllamaRequestOptions { Temperature = selection.Temperature.Value }
                    : null
            };

            using var content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{GetBaseUrl()}/api/generate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ollama generate request failed ({StatusCode}): {Body}", response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(SerializerOptions, cancellationToken);
            return result?.Response ?? string.Empty;
        }

        public Task<IReadOnlyCollection<LlmModelInfo>> ListModelsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
            => FetchModelsAsync(cancellationToken);

        public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{GetBaseUrl()}/api/tags", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ollama validation failed");
                return false;
            }
        }

        private async Task<IReadOnlyCollection<LlmModelInfo>> FetchModelsAsync(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"{GetBaseUrl()}/api/tags", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to fetch Ollama models ({StatusCode}): {Body}", response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }

            var tags = await response.Content.ReadFromJsonAsync<OllamaTagsResponse>(SerializerOptions, cancellationToken);
            var preferred = _options.CurrentValue.Ollama.PreferredModels ?? Array.Empty<string>();
            var models = tags?.Models?.Select(model => new LlmModelInfo(
                model.Name,
                model.Name,
                LlmProvider.Ollama,
                preferred.Contains(model.Name, StringComparer.OrdinalIgnoreCase),
                true)).ToList() ?? new List<LlmModelInfo>();

            // Ollama returns installed models; if preferred models are missing, still add them flagged unavailable.
            foreach (var preferredModel in preferred)
            {
                if (!models.Any(m => m.Name.Equals(preferredModel, StringComparison.OrdinalIgnoreCase)))
                {
                    models.Add(new LlmModelInfo(preferredModel, preferredModel, LlmProvider.Ollama, IsDefault: false, IsAvailable: false));
                }
            }

            return models;
        }

        private string GetBaseUrl()
        {
            var baseUrl = _options.CurrentValue.Ollama.BaseUrl;
            return string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:11434" : baseUrl.TrimEnd('/');
        }

        private sealed class OllamaRequest
        {
            public string Model { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public bool Stream { get; set; }
            public OllamaRequestOptions? Options { get; set; }
        }

        private sealed class OllamaRequestOptions
        {
            public float Temperature { get; set; }
        }

        private sealed class OllamaResponse
        {
            public string Response { get; set; } = string.Empty;
        }

        private sealed class OllamaTagsResponse
        {
            public List<OllamaModel>? Models { get; set; }
        }

        private sealed class OllamaModel
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
