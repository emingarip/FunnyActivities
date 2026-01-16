using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;
using FunnyActivities.Application.Commands.ContentGeneration;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.PromptTemplates;
using FunnyActivities.Domain.Interfaces;
using MediatR;

namespace FunnyActivities.Application.Handlers.ContentGeneration
{
    public class GenerateContentCommandHandler : IRequestHandler<GenerateContentCommand, string>
    {
        private readonly IPersonaRepository _personaRepository;
        private readonly FunnyActivities.Application.Interfaces.IActivityRepository _activityRepository;
        private readonly IAIService _aiService;
        private readonly IPromptTemplateService _promptTemplateService;

        public GenerateContentCommandHandler(
            IPersonaRepository personaRepository,
            FunnyActivities.Application.Interfaces.IActivityRepository activityRepository,
            IAIService aiService,
            IPromptTemplateService promptTemplateService)
        {
            _personaRepository = personaRepository;
            _activityRepository = activityRepository;
            _aiService = aiService;
            _promptTemplateService = promptTemplateService;
        }

        public async Task<string> Handle(GenerateContentCommand request, CancellationToken cancellationToken)
        {
            // Verify persona ownership
            var persona = await _personaRepository.GetByIdAsync(request.PersonaId);
            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            // Note: For content generation, we allow any authenticated user to use any persona
            // This enables sharing personas for content generation purposes
            // if (persona.UserId != request.UserId)
            // {
            //     throw new UnauthorizedAccessException("You do not have permission to use this persona.");
            // }

            // Get activity details
            var activity = await _activityRepository.GetByIdAsync(request.ActivityId);
            if (activity == null)
            {
                throw new KeyNotFoundException("Activity not found.");
            }

            // Build persona description
            var personaDescription = BuildPersonaDescription(persona);

            // Build activity description
            var activityDescription = activity.Description ?? activity.Name;

            var locale = !string.IsNullOrWhiteSpace(request.PromptLocale)
                ? request.PromptLocale!
                : CultureInfo.CurrentUICulture?.Name ?? "en-US";
            var scenario = GetScenarioName(request.ContentType);

            var renderContext = new PromptTemplateRenderContext
            {
                PersonaDescription = personaDescription,
                PersonaName = persona.Name,
                ActivityDescription = activityDescription,
                ActivityName = activity.Name,
                CustomPrompt = request.CustomPrompt,
                SystemPrompt = request.SystemPrompt,
                Locale = locale,
                Scenario = scenario
            };

            var renderResult = await _promptTemplateService.RenderAsync(request.PromptKey, renderContext, cancellationToken);
            var provider = ResolveProvider(request.Provider, renderResult.ProviderHint);
            var selection = new LlmSelection(
                provider,
                request.Model,
                request.Temperature,
                request.MaxTokens,
                request.SystemPrompt);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var content = await _aiService.GenerateContentAsync(renderResult.Prompt, selection, cancellationToken);
                stopwatch.Stop();

                await _promptTemplateService.LogAsync(new PromptCallLogEntry
                {
                    TemplateId = renderResult.Template?.Id,
                    TemplateKey = renderResult.Template?.Key ?? request.PromptKey ?? scenario,
                    Locale = locale,
                    Provider = selection.Provider.ToString(),
                    Model = selection.Model ?? string.Empty,
                    Duration = stopwatch.Elapsed.TotalMilliseconds,
                    Success = true,
                    ResultSummary = BuildResultSummary(content)
                }, cancellationToken);

                return content;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await _promptTemplateService.LogAsync(new PromptCallLogEntry
                {
                    TemplateId = renderResult.Template?.Id,
                    TemplateKey = renderResult.Template?.Key ?? request.PromptKey ?? scenario,
                    Locale = locale,
                    Provider = selection.Provider.ToString(),
                    Model = selection.Model ?? string.Empty,
                    Duration = stopwatch.Elapsed.TotalMilliseconds,
                    Success = false,
                    ErrorMessage = ex.Message
                }, cancellationToken);

                throw;
            }
        }

        private string BuildPersonaDescription(Domain.Entities.Persona persona)
        {
            var description = $"Name: {persona.Name}";

            if (!string.IsNullOrEmpty(persona.Description))
            {
                description += $"\nDescription: {persona.Description}";
            }

            if (persona.Characteristics.Any())
            {
                description += "\nCharacteristics:";
                foreach (var characteristic in persona.Characteristics.OrderBy(c => c.Order))
                {
                    description += $"\n- {characteristic.Name}: {characteristic.Value}";
                }
            }

            return description;
        }

        private static string GetScenarioName(ContentType contentType)
        {
            return contentType switch
            {
                ContentType.Story => "story",
                ContentType.Narrative => "narrative",
                ContentType.Tips => "tips",
                _ => "general"
            };
        }

        private static LlmProvider ResolveProvider(LlmProvider? requested, string? hint)
        {
            if (requested.HasValue)
            {
                return requested.Value;
            }

            if (!string.IsNullOrWhiteSpace(hint) && Enum.TryParse<LlmProvider>(hint, true, out var parsed))
            {
                return parsed;
            }

            return LlmProvider.Ollama;
        }

        private static string BuildResultSummary(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var normalized = content.Replace("\r\n", " ").Replace("\n", " ");
            return normalized.Length <= 240 ? normalized : normalized[..240];
        }
    }
}
