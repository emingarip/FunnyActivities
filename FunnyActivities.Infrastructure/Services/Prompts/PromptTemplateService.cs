using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;
using FunnyActivities.Application.DTOs.PromptTemplates;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.PromptTemplates;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FunnyActivities.Infrastructure.Services.AI;

namespace FunnyActivities.Infrastructure.Services.Prompts
{
    public class PromptTemplateService : IPromptTemplateService
    {
        private const string CacheKey = "prompt-templates-cache";
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);
        private static readonly Regex TokenRegex = new("{{\\s*(?<token>[A-Za-z0-9_\\.]+)\\s*}}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);

        private readonly IPromptTemplateRepository _templateRepository;
        private readonly IPromptCallLogRepository _logRepository;
        private readonly IAIService _aiService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PromptTemplateService> _logger;
        private readonly IOptionsMonitor<LlmOptions> _options;

        public PromptTemplateService(
            IPromptTemplateRepository templateRepository,
            IPromptCallLogRepository logRepository,
            IAIService aiService,
            IMemoryCache cache,
            ILogger<PromptTemplateService> logger,
            IOptionsMonitor<LlmOptions> options)
        {
            _templateRepository = templateRepository;
            _logRepository = logRepository;
            _aiService = aiService;
            _cache = cache;
            _logger = logger;
            _options = options;
        }

        public async Task<IReadOnlyList<PromptTemplateDto>> GetTemplatesAsync(string? locale = null, bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var normalizedLocale = string.IsNullOrWhiteSpace(locale) ? null : NormalizeLocale(locale);
            var templates = await LoadTemplatesAsync(cancellationToken);

            var filtered = templates.Where(t => includeInactive || t.IsActive);

            if (!string.IsNullOrWhiteSpace(normalizedLocale))
            {
                filtered = filtered.Where(t => string.Equals(t.Locale, normalizedLocale, StringComparison.OrdinalIgnoreCase));
            }

            return filtered
                .OrderBy(t => t.Key)
                .ThenByDescending(t => t.UpdatedAt)
                .Select(PromptTemplateMapper.ToDto)
                .ToList();
        }

        public async Task<PromptTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            return template == null ? null : PromptTemplateMapper.ToDto(template);
        }

        public async Task<PromptTemplateDto> CreateAsync(CreatePromptTemplateRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var key = NormalizeKey(request.Key);
            var locale = NormalizeLocale(request.Locale);
            await EnsureKeyUniqueAsync(key, locale, null, cancellationToken);

            var template = new PromptTemplate
            {
                Id = Guid.NewGuid(),
                Key = key,
                Title = request.Title.Trim(),
                Locale = locale,
                ProviderHint = NormalizeProviderHint(request.ProviderHint),
                Content = request.Content,
                OutputFormatHint = request.OutputFormatHint,
                Description = request.Description,
                IsActive = request.IsActive,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _templateRepository.AddAsync(template, cancellationToken);
            InvalidateCache();
            return PromptTemplateMapper.ToDto(template);
        }

        public async Task<PromptTemplateDto> UpdateAsync(Guid id, UpdatePromptTemplateRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
            {
                throw new KeyNotFoundException("Prompt template not found.");
            }

            var key = NormalizeKey(string.IsNullOrWhiteSpace(request.Key) ? template.Key : request.Key);
            var locale = NormalizeLocale(request.Locale);

            if (!string.Equals(template.Key, key, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(template.Locale, locale, StringComparison.OrdinalIgnoreCase))
            {
                await EnsureKeyUniqueAsync(key, locale, id, cancellationToken);
            }

            template.Key = key;
            template.Title = request.Title.Trim();
            template.Locale = locale;
            template.ProviderHint = NormalizeProviderHint(request.ProviderHint);
            template.Content = request.Content;
            template.OutputFormatHint = request.OutputFormatHint;
            template.Description = request.Description;
            template.IsActive = request.IsActive;
            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedBy = userId;

            await _templateRepository.UpdateAsync(template, cancellationToken);
            InvalidateCache();
            return PromptTemplateMapper.ToDto(template);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
            {
                return;
            }

            await _templateRepository.DeleteAsync(template, cancellationToken);
            InvalidateCache();
        }

        public async Task<PromptTemplateDto> CloneAsync(Guid id, ClonePromptTemplateRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
            {
                throw new KeyNotFoundException("Prompt template not found.");
            }

            var newKey = NormalizeKey(!string.IsNullOrWhiteSpace(request.Key)
                ? request.Key!
                : $"{template.Key}.copy");
            var newLocale = NormalizeLocale(request.Locale ?? template.Locale);

            await EnsureKeyUniqueAsync(newKey, newLocale, null, cancellationToken);

            var clone = new PromptTemplate
            {
                Id = Guid.NewGuid(),
                Key = newKey,
                Title = string.IsNullOrWhiteSpace(request.Title) ? $"{template.Title} (Clone)" : request.Title!.Trim(),
                Locale = newLocale,
                ProviderHint = template.ProviderHint,
                Content = template.Content,
                OutputFormatHint = template.OutputFormatHint,
                Description = template.Description,
                IsActive = request.IsActive ?? template.IsActive,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _templateRepository.AddAsync(clone, cancellationToken);
            InvalidateCache();
            return PromptTemplateMapper.ToDto(clone);
        }

        public async Task<PromptTemplateRenderResult> RenderAsync(string? key, PromptTemplateRenderContext context, CancellationToken cancellationToken = default)
        {
            var normalizedLocale = NormalizeLocale(context.Locale);
            var scenario = string.IsNullOrWhiteSpace(context.Scenario) ? "general" : context.Scenario;
            var resolvedKey = ResolveScenarioKey(key, scenario);

            var template = await ResolveTemplateAsync(resolvedKey, normalizedLocale, cancellationToken);
            var normalizedContext = new PromptTemplateRenderContext
            {
                PersonaDescription = context.PersonaDescription,
                PersonaName = context.PersonaName,
                ActivityDescription = context.ActivityDescription,
                ActivityName = context.ActivityName,
                CustomPrompt = context.CustomPrompt,
                SystemPrompt = context.SystemPrompt,
                Locale = normalizedLocale,
                Scenario = scenario,
                AdditionalVariables = context.AdditionalVariables
            };
            var prompt = RenderTemplateContent(template, normalizedContext);

            return new PromptTemplateRenderResult
            {
                Template = template.Id == Guid.Empty ? null : PromptTemplateMapper.ToDto(template),
                Prompt = prompt,
                ProviderHint = template.ProviderHint
            };
        }

        public async Task<PromptTemplateTestResultDto> TestAsync(string key, PromptTemplateTestRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var context = new PromptTemplateRenderContext
            {
                PersonaDescription = BuildPersonaBlock(request.PersonaName, request.PersonaDescription, request.PersonaTraits),
                PersonaName = request.PersonaName,
                ActivityDescription = BuildActivityBlock(request.ActivityName, request.ActivityDescription),
                ActivityName = request.ActivityName,
                CustomPrompt = request.CustomPrompt,
                SystemPrompt = request.SystemPrompt,
                Locale = NormalizeLocale(request.Locale ?? CultureInfo.CurrentUICulture?.Name ?? "en-US"),
                Scenario = "test",
                AdditionalVariables = request.AdditionalData
            };

            var renderResult = await RenderAsync(key, context, cancellationToken);
            var provider = ResolveProvider(request.Provider, renderResult.ProviderHint);
            var selection = new LlmSelection(provider, request.Model, null, null, request.SystemPrompt);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await _aiService.GenerateContentAsync(renderResult.Prompt, selection, cancellationToken);
                stopwatch.Stop();

                await LogAsync(new PromptCallLogEntry
                {
                    TemplateId = renderResult.Template?.Id,
                    TemplateKey = ResolveScenarioKey(key, context.Scenario),
                    Locale = context.Locale,
                    Provider = selection.Provider.ToString(),
                    Model = selection.Model ?? string.Empty,
                    Duration = stopwatch.Elapsed.TotalMilliseconds,
                    Success = true,
                    ResultSummary = BuildSummary(response),
                    IsTest = true
                }, cancellationToken);

                return new PromptTemplateTestResultDto
                {
                    Template = renderResult.Template,
                    Prompt = renderResult.Prompt,
                    Response = response,
                    Duration = stopwatch.Elapsed.TotalMilliseconds,
                    Provider = selection.Provider.ToString(),
                    Model = selection.Model ?? string.Empty
                };
            }
            catch (HttpRequestException httpEx)
            {
                stopwatch.Stop();
                var status = httpEx.StatusCode.HasValue ? ((int)httpEx.StatusCode.Value).ToString() : "HTTP";
                var friendly = $"LLM provider {selection.Provider} call failed ({status}). Ensure the model exists and the provider URL is reachable.";
                await LogAsync(new PromptCallLogEntry
                {
                    TemplateId = renderResult.Template?.Id,
                    TemplateKey = ResolveScenarioKey(key, context.Scenario),
                    Locale = context.Locale,
                    Provider = selection.Provider.ToString(),
                    Model = selection.Model ?? string.Empty,
                    Duration = stopwatch.Elapsed.TotalMilliseconds,
                    Success = false,
                    ErrorMessage = friendly,
                    IsTest = true
                }, cancellationToken);
                throw new InvalidOperationException(friendly, httpEx);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await LogAsync(new PromptCallLogEntry
                {
                    TemplateId = renderResult.Template?.Id,
                    TemplateKey = ResolveScenarioKey(key, context.Scenario),
                    Locale = context.Locale,
                    Provider = selection.Provider.ToString(),
                    Model = selection.Model ?? string.Empty,
                    Duration = stopwatch.Elapsed.TotalMilliseconds,
                    Success = false,
                    ErrorMessage = ex.Message,
                    IsTest = true
                }, cancellationToken);
                throw;
            }
        }

        public async Task<IReadOnlyList<PromptCallLogDto>> GetRecentLogsAsync(int take, CancellationToken cancellationToken = default)
        {
            var count = Math.Clamp(take, 1, 200);
            var logs = await _logRepository.GetRecentAsync(count, cancellationToken);
            return logs.Select(PromptTemplateMapper.ToDto).ToList();
        }

        public async Task LogAsync(PromptCallLogEntry logEntry, CancellationToken cancellationToken = default)
        {
            var log = new PromptCallLog
            {
                Id = Guid.NewGuid(),
                TemplateId = logEntry.TemplateId,
                TemplateKey = logEntry.TemplateKey,
                Locale = logEntry.Locale,
                Provider = logEntry.Provider,
                Model = logEntry.Model,
                Duration = logEntry.Duration,
                TokenUsage = logEntry.TokenUsage,
                Success = logEntry.Success,
                ResultSummary = logEntry.ResultSummary,
                ErrorMessage = logEntry.ErrorMessage,
                IsTest = logEntry.IsTest,
                CreatedAt = DateTime.UtcNow
            };

            await _logRepository.AddAsync(log, cancellationToken);
        }

        private async Task<IReadOnlyList<PromptTemplate>> LoadTemplatesAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CacheKey, out IReadOnlyList<PromptTemplate> cachedTemplates))
            {
                return cachedTemplates;
            }

            await _cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(CacheKey, out cachedTemplates))
                {
                    return cachedTemplates;
                }

                var templates = await _templateRepository.GetAllAsync(cancellationToken);
                if (templates == null || templates.Count == 0)
                {
                    templates = await SeedDefaultsAsync(cancellationToken);
                }

                _cache.Set(CacheKey, templates, CacheLifetime);
                return templates;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        private async Task<List<PromptTemplate>> SeedDefaultsAsync(CancellationToken cancellationToken)
        {
            var seeded = PromptTemplateDefaults.Seeds
                .Select(seed => new PromptTemplate
                {
                    Id = Guid.NewGuid(),
                    Key = NormalizeKey(seed.Key),
                    Title = seed.Title,
                    Locale = NormalizeLocale(seed.Locale),
                    ProviderHint = NormalizeProviderHint(seed.ProviderHint),
                    Content = seed.Content,
                    OutputFormatHint = seed.OutputFormatHint,
                    Description = seed.Description,
                    IsActive = seed.IsActive,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            await _templateRepository.AddRangeAsync(seeded, cancellationToken);
            return seeded;
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Template key is required.", nameof(key));
            }

            return key.Trim().ToLowerInvariant();
        }

        private static string NormalizeLocale(string? locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                return "en-US";
            }

            try
            {
                return CultureInfo.GetCultureInfo(locale.Trim()).Name;
            }
            catch (CultureNotFoundException)
            {
                return locale.Trim();
            }
        }

        private static string? NormalizeProviderHint(string? hint)
        {
            return string.IsNullOrWhiteSpace(hint) ? null : hint.Trim();
        }

        private async Task EnsureKeyUniqueAsync(string key, string locale, Guid? excludeId, CancellationToken cancellationToken)
        {
            var exists = await _templateRepository.ExistsWithKeyAsync(key, locale, excludeId, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"A template with key '{key}' already exists for locale '{locale}'.");
            }
        }

        private void InvalidateCache()
        {
            _cache.Remove(CacheKey);
        }

        private static string ResolveScenarioKey(string? requestedKey, string scenario)
        {
            if (!string.IsNullOrWhiteSpace(requestedKey))
            {
                return NormalizeKey(requestedKey);
            }

            return scenario.ToLowerInvariant() switch
            {
                "story" or "narrative" => PromptTemplateDefaults.StoryKey,
                "tips" => PromptTemplateDefaults.TipsKey,
                _ => PromptTemplateDefaults.GeneralKey
            };
        }

        private async Task<PromptTemplate> ResolveTemplateAsync(string key, string locale, CancellationToken cancellationToken)
        {
            var templates = await LoadTemplatesAsync(cancellationToken);

            var template = templates
                .Where(t => t.IsActive)
                .FirstOrDefault(t => string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase) &&
                                     string.Equals(t.Locale, locale, StringComparison.OrdinalIgnoreCase))
                ?? templates.FirstOrDefault(t => t.IsActive && string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase))
                ?? templates.FirstOrDefault(t => t.IsActive && string.Equals(t.Key, PromptTemplateDefaults.GeneralKey, StringComparison.OrdinalIgnoreCase) &&
                                                 string.Equals(t.Locale, locale, StringComparison.OrdinalIgnoreCase))
                ?? templates.FirstOrDefault(t => t.IsActive && string.Equals(t.Key, PromptTemplateDefaults.GeneralKey, StringComparison.OrdinalIgnoreCase));

            if (template == null)
            {
                // Fallback to in-memory seed if nothing is stored yet
                var seed = PromptTemplateDefaults.Seeds.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase)) ??
                           PromptTemplateDefaults.Seeds.First();

                template = new PromptTemplate
                {
                    Id = Guid.Empty,
                    Key = NormalizeKey(seed.Key),
                    Title = seed.Title,
                    Locale = NormalizeLocale(seed.Locale),
                    ProviderHint = seed.ProviderHint,
                    Content = seed.Content,
                    OutputFormatHint = seed.OutputFormatHint,
                    Description = seed.Description,
                    IsActive = true,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            return template;
        }

        private string RenderTemplateContent(PromptTemplate template, PromptTemplateRenderContext context)
        {
            var tokenValues = BuildTokenMap(template, context);
            var rendered = TokenRegex.Replace(template.Content, match =>
            {
                var token = match.Groups["token"].Value;
                return tokenValues.TryGetValue(token, out var value) ? value : string.Empty;
            });

            if (!string.IsNullOrWhiteSpace(template.OutputFormatHint))
            {
                rendered = $"{rendered.Trim()}{Environment.NewLine}{Environment.NewLine}{template.OutputFormatHint}";
            }

            return rendered.Trim();
        }

        private static Dictionary<string, string> BuildTokenMap(PromptTemplate template, PromptTemplateRenderContext context)
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PERSONA_DESCRIPTION"] = context.PersonaDescription ?? string.Empty,
                ["PERSONA_NAME"] = context.PersonaName ?? string.Empty,
                ["ACTIVITY_DESCRIPTION"] = context.ActivityDescription ?? string.Empty,
                ["ACTIVITY_NAME"] = context.ActivityName ?? string.Empty,
                ["CUSTOM_PROMPT"] = context.CustomPrompt ?? string.Empty,
                ["SYSTEM_PROMPT"] = context.SystemPrompt ?? string.Empty,
                ["SCENARIO"] = context.Scenario ?? "general",
                ["LOCALE"] = context.Locale,
                ["TEMPLATE_TITLE"] = template.Title,
                ["CURRENT_UTC"] = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture)
            };

            if (context.AdditionalVariables != null)
            {
                foreach (var pair in context.AdditionalVariables)
                {
                    tokens[pair.Key] = pair.Value;
                }
            }

            return tokens;
        }

        private LlmProvider ResolveProvider(string? provider, string? hint)
        {
            if (!string.IsNullOrWhiteSpace(provider) && Enum.TryParse(provider, true, out LlmProvider parsed))
            {
                return parsed;
            }

            if (!string.IsNullOrWhiteSpace(hint) && Enum.TryParse(hint, true, out LlmProvider hinted))
            {
                return hinted;
            }

            return _options.CurrentValue.DefaultProvider;
        }

        private static string BuildPersonaBlock(string name, string description, IDictionary<string, string>? traits)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.AppendLine($"Name: {name}");
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.AppendLine($"Description: {description}");
            }

            if (traits != null && traits.Any())
            {
                builder.AppendLine("Traits:");
                foreach (var trait in traits)
                {
                    builder.AppendLine($"- {trait.Key}: {trait.Value}");
                }
            }

            return builder.ToString().Trim();
        }

        private static string BuildActivityBlock(string name, string description)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.AppendLine($"Activity: {name}");
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.AppendLine(description);
            }

            return builder.ToString().Trim();
        }

        private static string BuildSummary(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var normalized = content.Replace("\r\n", " ").Replace("\n", " ");
            return normalized.Length <= 240 ? normalized : normalized.Substring(0, 240);
        }
    }
}
