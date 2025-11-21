using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Infrastructure.Services.AI
{
    public class OpenAiChatProvider : ILlmProvider
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAiChatProvider> _logger;
        private readonly IOptionsMonitor<LlmOptions> _options;

        public OpenAiChatProvider(HttpClient httpClient, ILogger<OpenAiChatProvider> logger, IOptionsMonitor<LlmOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options;
        }

        public LlmProvider ProviderId => LlmProvider.OpenAI;

        public async Task<string> GenerateAsync(string prompt, LlmSelection selection, CancellationToken cancellationToken = default)
        {
            var openAi = _options.CurrentValue.OpenAI;
            EnsureConfigured(openAi);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{GetBaseUrl(openAi)}/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAi.ApiKey);
            if (!string.IsNullOrWhiteSpace(openAi.OrganizationId))
            {
                request.Headers.Add("OpenAI-Organization", openAi.OrganizationId);
            }

            var payload = new
            {
                model = selection.Model ?? openAi.DefaultModel,
                temperature = selection.Temperature ?? 0.7f,
                max_tokens = selection.MaxTokens,
                messages = new[]
                {
                    new { role = "system", content = selection.SystemPrompt ?? openAi.DefaultSystemPrompt },
                    new { role = "user", content = prompt }
                }
            };

            request.Content = JsonContent.Create(payload, options: SerializerOptions);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI completion failed ({StatusCode}): {Body}", response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }

            var completion = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(SerializerOptions, cancellationToken);
            return completion?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }

        public async Task<IReadOnlyCollection<LlmModelInfo>> ListModelsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            var openAi = _options.CurrentValue.OpenAI;
            EnsureConfigured(openAi);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{GetBaseUrl(openAi)}/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAi.ApiKey);
            if (!string.IsNullOrWhiteSpace(openAi.OrganizationId))
            {
                request.Headers.Add("OpenAI-Organization", openAi.OrganizationId);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("OpenAI models endpoint failed ({StatusCode}): {Body}", response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }

            var models = await response.Content.ReadFromJsonAsync<OpenAiModelsResponse>(SerializerOptions, cancellationToken);
            var allowed = openAi.AllowedModels?.Where(m => !string.IsNullOrWhiteSpace(m)).ToArray() ?? Array.Empty<string>();
            var filtered = models?.Data?
                .Where(model => allowed.Length == 0 || allowed.Contains(model.Id, StringComparer.OrdinalIgnoreCase))
                .Select(model => new LlmModelInfo(model.Id, model.Id, LlmProvider.OpenAI,
                    string.Equals(model.Id, openAi.DefaultModel, StringComparison.OrdinalIgnoreCase)))
                .ToList() ?? new List<LlmModelInfo>();

            if (filtered.Count == 0 && allowed.Length > 0)
            {
                filtered.AddRange(allowed.Select(modelId => new LlmModelInfo(modelId, modelId, LlmProvider.OpenAI,
                    string.Equals(modelId, openAi.DefaultModel, StringComparison.OrdinalIgnoreCase), false)));
            }

            return filtered;
        }

        public async Task<bool> ValidateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var models = await ListModelsAsync(false, cancellationToken);
                return models.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI validation failed");
                return false;
            }
        }

        private static string GetBaseUrl(OpenAiOptions options)
            => (options.BaseUrl ?? "https://api.openai.com/v1").TrimEnd('/');

        private static void EnsureConfigured(OpenAiOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured.");
            }
        }

        private sealed class ChatCompletionResponse
        {
            public List<ChatChoice>? Choices { get; set; }
        }

        private sealed class ChatChoice
        {
            public ChatMessage? Message { get; set; }
        }

        private sealed class ChatMessage
        {
            public string? Content { get; set; }
        }

        private sealed class OpenAiModelsResponse
        {
            public List<OpenAiModel>? Data { get; set; }
        }

        private sealed class OpenAiModel
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}
