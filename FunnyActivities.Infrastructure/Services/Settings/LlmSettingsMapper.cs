using System;
using System.Linq;
using System.Text.Json;
using FunnyActivities.Application.AI;
using FunnyActivities.Application.DTOs.Settings;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Infrastructure.Services.AI;

namespace FunnyActivities.Infrastructure.Services.Settings
{
    internal static class LlmSettingsMapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);

        public static LlmSettingsDto ToDto(LlmSetting entity)
        {
            return new LlmSettingsDto
            {
                DefaultProvider = entity.DefaultProvider,
                DefaultModel = entity.DefaultModel,
                OllamaBaseUrl = entity.OllamaBaseUrl,
                OllamaHealthCheckModel = entity.OllamaHealthCheckModel,
                OllamaPreferredModels = DeserializeArray(entity.OllamaPreferredModelsJson),
                OpenAiBaseUrl = entity.OpenAiBaseUrl,
                OpenAiDefaultModel = entity.OpenAiDefaultModel,
                OpenAiAllowedModels = DeserializeArray(entity.OpenAiAllowedModelsJson),
                OpenAiOrganizationId = entity.OpenAiOrganizationId ?? string.Empty,
                HasOpenAiApiKey = !string.IsNullOrWhiteSpace(entity.OpenAiApiKey),
                ModelCacheSeconds = entity.ModelCacheSeconds
            };
        }

        public static void ApplyUpdate(LlmSetting entity, UpdateLlmSettingsRequest request)
        {
            entity.DefaultProvider = string.IsNullOrWhiteSpace(request.DefaultProvider)
                ? entity.DefaultProvider
                : request.DefaultProvider;
            entity.DefaultModel = string.IsNullOrWhiteSpace(request.DefaultModel) ? entity.DefaultModel : request.DefaultModel;
            entity.OllamaBaseUrl = string.IsNullOrWhiteSpace(request.OllamaBaseUrl) ? entity.OllamaBaseUrl : request.OllamaBaseUrl;
            entity.OllamaHealthCheckModel = string.IsNullOrWhiteSpace(request.OllamaHealthCheckModel)
                ? entity.OllamaHealthCheckModel
                : request.OllamaHealthCheckModel;
            entity.OllamaPreferredModelsJson = SerializeArray(request.OllamaPreferredModels);
            entity.OpenAiBaseUrl = string.IsNullOrWhiteSpace(request.OpenAiBaseUrl) ? entity.OpenAiBaseUrl : request.OpenAiBaseUrl;
            entity.OpenAiDefaultModel = string.IsNullOrWhiteSpace(request.OpenAiDefaultModel)
                ? entity.OpenAiDefaultModel
                : request.OpenAiDefaultModel;
            entity.OpenAiAllowedModelsJson = SerializeArray(request.OpenAiAllowedModels);
            entity.OpenAiOrganizationId = request.OpenAiOrganizationId ?? string.Empty;
            if (request.OpenAiApiKey != null)
            {
                entity.OpenAiApiKey = request.OpenAiApiKey;
            }
            entity.ModelCacheSeconds = request.ModelCacheSeconds > 0 ? request.ModelCacheSeconds : entity.ModelCacheSeconds;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        public static LlmOptions ToOptions(LlmSetting entity)
        {
            return new LlmOptions
            {
                DefaultProvider = ParseProvider(entity.DefaultProvider),
                DefaultModel = entity.DefaultModel,
                ModelCacheSeconds = entity.ModelCacheSeconds,
                Ollama = new OllamaOptions
                {
                    BaseUrl = entity.OllamaBaseUrl,
                    HealthCheckModel = entity.OllamaHealthCheckModel,
                    PreferredModels = DeserializeArray(entity.OllamaPreferredModelsJson)
                },
                OpenAI = new OpenAiOptions
                {
                    BaseUrl = entity.OpenAiBaseUrl,
                    ApiKey = entity.OpenAiApiKey ?? string.Empty,
                    OrganizationId = entity.OpenAiOrganizationId ?? string.Empty,
                    DefaultModel = entity.OpenAiDefaultModel,
                    AllowedModels = DeserializeArray(entity.OpenAiAllowedModelsJson)
                }
            };
        }

        public static LlmSetting FromOptions(LlmOptions options)
        {
            return new LlmSetting
            {
                DefaultProvider = options.DefaultProvider.ToString(),
                DefaultModel = options.DefaultModel,
                ModelCacheSeconds = options.ModelCacheSeconds,
                OllamaBaseUrl = options.Ollama.BaseUrl,
                OllamaHealthCheckModel = options.Ollama.HealthCheckModel,
                OllamaPreferredModelsJson = SerializeArray(options.Ollama.PreferredModels),
                OpenAiBaseUrl = options.OpenAI.BaseUrl,
                OpenAiDefaultModel = options.OpenAI.DefaultModel,
                OpenAiAllowedModelsJson = SerializeArray(options.OpenAI.AllowedModels),
                OpenAiOrganizationId = options.OpenAI.OrganizationId,
                OpenAiApiKey = options.OpenAI.ApiKey
            };
        }

        public static string[] DeserializeArray(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<string[]>(value, JsonOptions) ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public static string SerializeArray(string[]? values)
        {
            return JsonSerializer.Serialize(values ?? Array.Empty<string>(), JsonOptions);
        }

        private static LlmProvider ParseProvider(string? value)
        {
            return Enum.TryParse<LlmProvider>(value, true, out var provider)
                ? provider
                : LlmProvider.Ollama;
        }
    }
}
