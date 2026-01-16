using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IPromptCallLogRepository
    {
        Task AddAsync(PromptCallLog log, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PromptCallLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    }
}
