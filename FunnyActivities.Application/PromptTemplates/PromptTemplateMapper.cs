using FunnyActivities.Application.DTOs.PromptTemplates;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.PromptTemplates
{
    public static class PromptTemplateMapper
    {
        public static PromptTemplateDto ToDto(PromptTemplate entity)
        {
            return new PromptTemplateDto
            {
                Id = entity.Id,
                Key = entity.Key,
                Title = entity.Title,
                Locale = entity.Locale,
                ProviderHint = entity.ProviderHint,
                Content = entity.Content,
                OutputFormatHint = entity.OutputFormatHint,
                IsActive = entity.IsActive,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy,
                Description = entity.Description
            };
        }

        public static PromptCallLogDto ToDto(PromptCallLog entity)
        {
            return new PromptCallLogDto
            {
                Id = entity.Id,
                TemplateId = entity.TemplateId,
                TemplateKey = entity.TemplateKey,
                Locale = entity.Locale,
                Provider = entity.Provider,
                Model = entity.Model,
                Duration = entity.Duration,
                TokenUsage = entity.TokenUsage,
                Success = entity.Success,
                ResultSummary = entity.ResultSummary,
                ErrorMessage = entity.ErrorMessage,
                IsTest = entity.IsTest,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
