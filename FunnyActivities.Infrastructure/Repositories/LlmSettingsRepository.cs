using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FunnyActivities.Infrastructure.Repositories
{
    public class LlmSettingsRepository : ILlmSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public LlmSettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LlmSetting?> GetAsync(CancellationToken cancellationToken = default)
        {
            return await _context.LlmSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpsertAsync(LlmSetting settings, CancellationToken cancellationToken = default)
        {
            var existing = await _context.LlmSettings.FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                _context.LlmSettings.Add(settings);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(settings);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
