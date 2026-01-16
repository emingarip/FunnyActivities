using System.Threading;
using System.Threading.Tasks;

namespace FunnyActivities.Application.Interfaces
{
    public interface ILlmSettingsInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
