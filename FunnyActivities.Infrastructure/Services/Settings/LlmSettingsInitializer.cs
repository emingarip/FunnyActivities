using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Infrastructure.Services.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Infrastructure.Services.Settings
{
    public class LlmSettingsInitializer : ILlmSettingsInitializer
    {
        private readonly ILlmSettingsRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly IOptionsMonitorCache<LlmOptions> _optionsCache;

        public LlmSettingsInitializer(
            ILlmSettingsRepository repository,
            IConfiguration configuration,
            IOptionsMonitorCache<LlmOptions> optionsCache)
        {
            _repository = repository;
            _configuration = configuration;
            _optionsCache = optionsCache;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetAsync(cancellationToken);
            if (existing == null)
            {
                var configOptions = _configuration.GetSection("Llm").Get<LlmOptions>() ?? new LlmOptions();
                var entity = LlmSettingsMapper.FromOptions(configOptions);
                await _repository.UpsertAsync(entity, cancellationToken);
                RefreshOptions(entity);
            }
            else
            {
                RefreshOptions(existing);
            }
        }

        private void RefreshOptions(LlmSetting entity)
        {
            var options = LlmSettingsMapper.ToOptions(entity);
            _optionsCache.TryRemove(Options.DefaultName);
            _optionsCache.TryAdd(Options.DefaultName, options);
        }
    }
}
