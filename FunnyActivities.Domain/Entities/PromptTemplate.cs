using System;

namespace FunnyActivities.Domain.Entities
{
    public class PromptTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Locale { get; set; } = "en-US";
        public string? ProviderHint { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? OutputFormatHint { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public string? Description { get; set; }
    }
}
