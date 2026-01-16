using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface ILlmSettingsRepository
    {
        Task<LlmSetting?> GetAsync(CancellationToken cancellationToken = default);
        Task UpsertAsync(LlmSetting settings, CancellationToken cancellationToken = default);
    }
}
