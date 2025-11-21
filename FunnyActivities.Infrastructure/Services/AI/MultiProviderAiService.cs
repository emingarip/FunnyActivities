using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;
using FunnyActivities.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Infrastructure.Services.AI
{
    public class MultiProviderAiService : IAIService
    {
        private readonly IReadOnlyDictionary<LlmProvider, ILlmProvider> _providers;
        private readonly ILogger<MultiProviderAiService> _logger;
        private readonly IOptionsMonitor<LlmOptions> _options;
        private readonly IMemoryCache _cache;

        public MultiProviderAiService(
            IEnumerable<ILlmProvider> providers,
            ILogger<MultiProviderAiService> logger,
            IOptionsMonitor<LlmOptions> options,
            IMemoryCache cache)
        {
            _providers = providers.ToDictionary(p => p.ProviderId);
            _logger = logger;
            _options = options;
            _cache = cache;
        }

        public Task<string> GenerateContentAsync(string prompt, LlmSelection selection, CancellationToken cancellationToken = default)
            => ExecuteGenerateAsync(prompt, selection, cancellationToken);

        public Task<string> GeneratePersonaContentAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default)
        {
            var prompt = $"Based on this persona: {personaDescription}\n\nGenerate engaging content for this activity: {activityDescription}\n\nMake it personalized and fun.";
            return ExecuteGenerateAsync(prompt, selection, cancellationToken);
        }

        public Task<string> GenerateStoryAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default)
        {
            var prompt = $"Create an engaging story based on this persona: {personaDescription}\n\nThe story should revolve around this activity: {activityDescription}\n\nMake it narrative-driven, immersive, and personalized to the persona's characteristics. Include a beginning, middle, and end.";
            return ExecuteGenerateAsync(prompt, selection, cancellationToken);
        }

        public Task<string> GenerateNarrativeAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default)
        {
            var prompt = $"Write a compelling narrative from the perspective of this persona: {personaDescription}\n\nThe narrative should describe their experience with this activity: {activityDescription}\n\nUse first-person perspective and make it vivid and engaging.";
            return ExecuteGenerateAsync(prompt, selection, cancellationToken);
        }

        public Task<string> GenerateTipsAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default)
        {
            var prompt = $"Based on this persona: {personaDescription}\n\nProvide personalized tips and advice for successfully completing this activity: {activityDescription}\n\nTailor the tips to the persona's characteristics and make them practical and actionable.";
            return ExecuteGenerateAsync(prompt, selection, cancellationToken);
        }

        public async Task<bool> ValidateConnectionAsync(LlmProvider provider, CancellationToken cancellationToken = default)
        {
            try
            {
                var resolved = ResolveProvider(provider);
                return await resolved.ValidateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation failed for provider {Provider}", provider);
                return false;
            }
        }

        public async Task<IEnumerable<LlmModelInfo>> ListAvailableModelsAsync(LlmProvider provider, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"llm-models-{provider}";
            if (!forceRefresh && _cache.TryGetValue(cacheKey, out IReadOnlyCollection<LlmModelInfo> cached))
            {
                return cached;
            }

            var resolved = ResolveProvider(provider);
            var models = await resolved.ListModelsAsync(forceRefresh, cancellationToken);

            var cacheDuration = Math.Max(_options.CurrentValue.ModelCacheSeconds, 30);
            _cache.Set(cacheKey, models, TimeSpan.FromSeconds(cacheDuration));
            return models;
        }

        private Task<string> ExecuteGenerateAsync(string prompt, LlmSelection selection, CancellationToken cancellationToken)
        {
            var normalized = NormalizeSelection(selection);
            var provider = ResolveProvider(normalized.Provider);
            return provider.GenerateAsync(prompt, normalized, cancellationToken);
        }

        private ILlmProvider ResolveProvider(LlmProvider provider)
        {
            if (_providers.TryGetValue(provider, out var resolved))
            {
                return resolved;
            }

            throw new InvalidOperationException($"LLM provider '{provider}' is not configured.");
        }

        private LlmSelection NormalizeSelection(LlmSelection selection)
        {
            var options = _options.CurrentValue;
            var baseSelection = selection ?? new LlmSelection(options.DefaultProvider);
            var provider = baseSelection.Provider;
            var model = !string.IsNullOrWhiteSpace(baseSelection.Model)
                ? baseSelection.Model
                : GetDefaultModel(provider, options);
            return baseSelection with { Provider = provider, Model = model };
        }

        private static string GetDefaultModel(LlmProvider provider, LlmOptions options)
        {
            return provider switch
            {
                LlmProvider.OpenAI => !string.IsNullOrWhiteSpace(options.OpenAI.DefaultModel)
                    ? options.OpenAI.DefaultModel
                    : options.DefaultModel,
                _ => !string.IsNullOrWhiteSpace(options.Ollama.HealthCheckModel)
                    ? options.Ollama.HealthCheckModel
                    : options.DefaultModel
            };
        }
    }
}
