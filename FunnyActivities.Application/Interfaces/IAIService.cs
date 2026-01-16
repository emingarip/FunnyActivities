using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.AI;

namespace FunnyActivities.Application.Interfaces
{
    public interface IAIService
    {
        Task<string> GenerateContentAsync(string prompt, LlmSelection selection, CancellationToken cancellationToken = default);
        Task<string> GeneratePersonaContentAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default);
        Task<string> GenerateStoryAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default);
        Task<string> GenerateNarrativeAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default);
        Task<string> GenerateTipsAsync(string personaDescription, string activityDescription, LlmSelection selection, CancellationToken cancellationToken = default);
        Task<bool> ValidateConnectionAsync(LlmProvider provider, CancellationToken cancellationToken = default);
        Task<IEnumerable<LlmModelInfo>> ListAvailableModelsAsync(LlmProvider provider, bool forceRefresh = false, CancellationToken cancellationToken = default);
    }
}
