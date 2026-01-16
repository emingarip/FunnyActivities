using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.DTOs.Settings;

namespace FunnyActivities.Application.Interfaces
{
    public interface ILlmSettingsService
    {
        Task<LlmSettingsDto> GetAsync(CancellationToken cancellationToken = default);
        Task<LlmSettingsDto> UpdateAsync(UpdateLlmSettingsRequest request, Guid userId, CancellationToken cancellationToken = default);
    }
}
