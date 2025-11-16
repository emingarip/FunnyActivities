using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Application.Commands.ContentGeneration;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.ContentGeneration
{
    public class GenerateContentCommandHandler : IRequestHandler<GenerateContentCommand, string>
    {
        private readonly IPersonaRepository _personaRepository;
        private readonly FunnyActivities.Application.Interfaces.IActivityRepository _activityRepository;
        private readonly IAIService _aiService;

        public GenerateContentCommandHandler(
            IPersonaRepository personaRepository,
            FunnyActivities.Application.Interfaces.IActivityRepository activityRepository,
            IAIService aiService)
        {
            _personaRepository = personaRepository;
            _activityRepository = activityRepository;
            _aiService = aiService;
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

            // Generate content based on content type
            string content;
            if (!string.IsNullOrEmpty(request.CustomPrompt))
            {
                // Use custom prompt with persona and activity context
                var enhancedPrompt = $"{request.CustomPrompt}\n\nPersona: {personaDescription}\nActivity: {activityDescription}";
                content = await _aiService.GenerateContentAsync(enhancedPrompt, request.Model);
            }
            else
            {
                // Use specific content type generation
                content = request.ContentType switch
                {
                    ContentType.Story => await _aiService.GenerateStoryAsync(personaDescription, activityDescription, request.Model),
                    ContentType.Narrative => await _aiService.GenerateNarrativeAsync(personaDescription, activityDescription, request.Model),
                    ContentType.Tips => await _aiService.GenerateTipsAsync(personaDescription, activityDescription, request.Model),
                    _ => await _aiService.GeneratePersonaContentAsync(personaDescription, activityDescription, request.Model)
                };
            }

            return content;
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
    }
}