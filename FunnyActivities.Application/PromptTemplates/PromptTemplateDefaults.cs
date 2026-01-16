using System;
using System.Collections.Generic;
using System.Linq;

namespace FunnyActivities.Application.PromptTemplates
{
    public static class PromptTemplateDefaults
    {
        public const string GeneralKey = "persona.general";
        public const string StoryKey = "persona.story";
        public const string TipsKey = "persona.tips";

        public static IReadOnlyList<PromptTemplateSeed> Seeds { get; } = new[]
        {
            new PromptTemplateSeed
            {
                Key = GeneralKey,
                Title = "Persona Draft (Markdown)",
                Locale = "en-US",
                ProviderHint = "Ollama",
                Content =
@"You are an empathetic creative strategist that helps product teams turn personas into engaging activity drafts.

Persona details:
{{PERSONA_DESCRIPTION}}

Activity context:
{{ACTIVITY_DESCRIPTION}}

{{CUSTOM_PROMPT}}

Produce friendly, encouraging copy in {{LOCALE}} that explains why the persona would enjoy the activity. Reference scenario '{{SCENARIO}}'.",
                OutputFormatHint = "Respond in Markdown with headings: Persona Hook, Personalized Highlights (bullet points), Closing CTA.",
                Description = "Warm intro format for general persona drafts.",
                IsActive = true
            },
            new PromptTemplateSeed
            {
                Key = StoryKey,
                Title = "Persona Story (First Person)",
                Locale = "en-US",
                ProviderHint = "OpenAI",
                Content =
@"You are a narrative designer. Write a first-person short story told by {{PERSONA_NAME}} about completing {{ACTIVITY_NAME}}.

Use these canonical details:
Persona: {{PERSONA_DESCRIPTION}}
Activity: {{ACTIVITY_DESCRIPTION}}

{{CUSTOM_PROMPT}}

Keep the voice consistent with the persona and infuse sensory details. Mention locale {{LOCALE}} at least once.",
                OutputFormatHint = "Return Markdown with sections: Opening Scene, Challenge, Breakthrough, Resolution.",
                Description = "Storytelling-focused prompt geared toward OpenAI style models.",
                IsActive = true
            },
            new PromptTemplateSeed
            {
                Key = TipsKey,
                Title = "Persona Tips Checklist",
                Locale = "en-US",
                ProviderHint = "Ollama",
                Content =
@"Craft five numbered tips for {{PERSONA_NAME}} so they can excel at {{ACTIVITY_NAME}}.

Context for tone and constraints:
Persona traits -> {{PERSONA_DESCRIPTION}}
Activity background -> {{ACTIVITY_DESCRIPTION}}

{{CUSTOM_PROMPT}}

Each tip must include a friendly label and a practical action. Close with one motivational sentence.",
                OutputFormatHint = "Return plain text with numbered tips plus a final encouragement sentence.",
                Description = "Short checklist prompt for guiding personas.",
                IsActive = true
            }
        };

        public static PromptTemplateSeed? Find(string key, string locale)
        {
            return Seeds.FirstOrDefault(seed =>
                string.Equals(seed.Key, key, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(seed.Locale, locale, StringComparison.OrdinalIgnoreCase));
        }
    }
}
