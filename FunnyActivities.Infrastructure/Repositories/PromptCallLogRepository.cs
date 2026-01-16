using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure.Repositories
{
    public class PromptCallLogRepository : IPromptCallLogRepository
    {
        private readonly ApplicationDbContext _context;

        public PromptCallLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PromptCallLog log, CancellationToken cancellationToken = default)
        {
            _context.PromptCallLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PromptCallLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.PromptCallLogs
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
