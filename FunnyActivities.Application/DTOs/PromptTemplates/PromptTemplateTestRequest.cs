using System.Collections.Generic;
using FunnyActivities.Application.AI;

namespace FunnyActivities.Application.DTOs.PromptTemplates
{
    public class PromptTemplateTestRequest
    {
        public string PersonaName { get; set; } = "Ava Explorer";
        public string PersonaDescription { get; set; } = "A curious storyteller who loves connecting people with playful adventures.";
        public IDictionary<string, string>? PersonaTraits { get; set; }
        public string ActivityName { get; set; } = "Neighborhood Treasure Hunt";
        public string ActivityDescription { get; set; } = "A collaborative outdoor game filled with riddles, creative clues, and social challenges.";
        public string? CustomPrompt { get; set; }
        public string? SystemPrompt { get; set; }
        public string? Locale { get; set; }
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public IDictionary<string, string>? AdditionalData { get; set; }
    }
}
