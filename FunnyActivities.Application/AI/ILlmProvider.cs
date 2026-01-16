using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FunnyActivities.Application.AI
{
    /// <summary>
    /// Her LLM sağlayıcısı için ortak sözleşme.
    /// </summary>
    public interface ILlmProvider
    {
        LlmProvider ProviderId { get; }

        Task<string> GenerateAsync(string prompt, LlmSelection selection, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<LlmModelInfo>> ListModelsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

        Task<bool> ValidateAsync(CancellationToken cancellationToken = default);
    }
}
