using System;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.DTOs.Settings;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Infrastructure.Services.AI;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Infrastructure.Services.Settings
{
    public class LlmSettingsService : ILlmSettingsService
    {
        private readonly ILlmSettingsRepository _repository;
        private readonly IOptionsMonitorCache<LlmOptions> _optionsCache;

        public LlmSettingsService(
            ILlmSettingsRepository repository,
            IOptionsMonitorCache<LlmOptions> optionsCache)
        {
            _repository = repository;
            _optionsCache = optionsCache;
        }

        public async Task<LlmSettingsDto> GetAsync(CancellationToken cancellationToken = default)
        {
            var entity = await GetOrCreateAsync(cancellationToken);
            return LlmSettingsMapper.ToDto(entity);
        }

        public async Task<LlmSettingsDto> UpdateAsync(UpdateLlmSettingsRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var entity = await GetOrCreateAsync(cancellationToken);

            LlmSettingsMapper.ApplyUpdate(entity, request);
            entity.UpdatedBy = userId;

            await _repository.UpsertAsync(entity, cancellationToken);
            RefreshOptions(entity);

            return LlmSettingsMapper.ToDto(entity);
        }

        private async Task<LlmSetting> GetOrCreateAsync(CancellationToken cancellationToken)
        {
            var entity = await _repository.GetAsync(cancellationToken);
            if (entity != null)
            {
                return entity;
            }

            entity = new LlmSetting
            {
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.UpsertAsync(entity, cancellationToken);
            RefreshOptions(entity);
            return entity;
        }

        private void RefreshOptions(LlmSetting entity)
        {
            var options = LlmSettingsMapper.ToOptions(entity);
            _optionsCache.TryRemove(Options.DefaultName);
            _optionsCache.TryAdd(Options.DefaultName, options);
        }
    }
}
