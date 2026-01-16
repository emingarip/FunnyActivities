using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.DTOs.PromptTemplates;
using FunnyActivities.Application.PromptTemplates;

namespace FunnyActivities.Application.Interfaces
{
    public interface IPromptTemplateService
    {
        Task<IReadOnlyList<PromptTemplateDto>> GetTemplatesAsync(string? locale = null, bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<PromptTemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PromptTemplateDto> CreateAsync(CreatePromptTemplateRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<PromptTemplateDto> UpdateAsync(Guid id, UpdatePromptTemplateRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PromptTemplateDto> CloneAsync(Guid id, ClonePromptTemplateRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<PromptTemplateRenderResult> RenderAsync(string? key, PromptTemplateRenderContext context, CancellationToken cancellationToken = default);
        Task<PromptTemplateTestResultDto> TestAsync(string key, PromptTemplateTestRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PromptCallLogDto>> GetRecentLogsAsync(int take, CancellationToken cancellationToken = default);
        Task LogAsync(PromptCallLogEntry logEntry, CancellationToken cancellationToken = default);
    }
}
