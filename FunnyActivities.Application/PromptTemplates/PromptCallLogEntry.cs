using System;

namespace FunnyActivities.Application.PromptTemplates
{
    public class PromptCallLogEntry
    {
        public Guid? TemplateId { get; set; }
        public string TemplateKey { get; set; } = string.Empty;
        public string? Locale { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public double Duration { get; set; }
        public int? TokenUsage { get; set; }
        public bool Success { get; set; }
        public string? ResultSummary { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsTest { get; set; }
    }
}
