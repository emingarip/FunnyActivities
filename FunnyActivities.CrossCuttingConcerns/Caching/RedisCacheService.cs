using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunnyActivities.CrossCuttingConcerns.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _cache.GetStringAsync(key).ConfigureAwait(false);
            if (value == null)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry;
            }
            else
            {
                // Default expiry of 30 minutes
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            }

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options).ConfigureAwait(false);
            _logger.LogDebug("Cache set for key: {Key} with expiry: {Expiry}", key, expiry ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key).ConfigureAwait(false);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await _cache.GetAsync(key).ConfigureAwait(false);
            var exists = value != null;
            _logger.LogDebug("Cache exists check for key: {Key} - {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    // Cache invalidation strategies
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // Note: Redis doesn't have native pattern deletion in IDistributedCache
            // This is a simplified implementation - in production, you'd use IConnectionMultiplexer directly
            _logger.LogInformation("Pattern-based cache invalidation requested for pattern: {Pattern}", pattern);
            // For now, just log - implement actual pattern deletion using Redis SCAN/DEL if needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pattern-based cache invalidation for pattern: {Pattern}", pattern);
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            _logger.LogWarning("Clearing all cache entries");
            // Note: This is not directly supported by IDistributedCache
            // In production, you'd need to use IConnectionMultiplexer.FLUSHDB
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache entries");
            throw;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        var cached = await GetAsync<T>(key).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory().ConfigureAwait(false);
        await SetAsync(key, value, expiry).ConfigureAwait(false);
        return value;
    }

    public async Task<IDictionary<string, T>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T>();
        foreach (var key in keys)
        {
            var value = await GetAsync<T>(key).ConfigureAwait(false);
            if (value != null)
            {
                result[key] = value;
            }
        }
        return result;
    }

    public async Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null)
    {
        foreach (var kvp in keyValuePairs)
        {
            await SetAsync(kvp.Key, kvp.Value, expiry).ConfigureAwait(false);
        }
    }
}