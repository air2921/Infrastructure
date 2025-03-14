using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.DistributedCache;

public class CacheClient(IDistributedCache cache, ILogger<CacheClient> logger, CacheConfigureOptions configureOptions) : ICacheClient
{
    public async Task<TResult?> GetAsync<TResult>(string key, CancellationToken cancellationToken = default) where TResult : class
    {
        try
        {
            logger.LogInformation($"Gets object associated with key {key}");

            object? cacheObj = await cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (cacheObj is null)
                return null;

            if (typeof(TResult) == typeof(string))
            {
                if (cacheObj is null)
                    return null;

                return (TResult?)cacheObj;
            }

            return JsonSerializer.Deserialize<TResult>((string)cacheObj);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new DistributedCacheException();
        }
    }


    public async Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, TimeSpan sliding, CancellationToken cancellationToken = default) where TValue : notnull
    {
        try
        {
            var json = JsonSerializer.Serialize(value, configureOptions.JsonSerializerSettings);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absolute)
                .SetSlidingExpiration(sliding);

            await cache.SetStringAsync(key, json, options, cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                $"Set object in cache associative key {key}, absolute expiration {absolute.Seconds} seconds, sliding expiration {sliding.Seconds} seconds");
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new DistributedCacheException();
        }
    }

    public async Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, CancellationToken cancellationToken = default) where TValue : notnull
    {
        try
        {
            var json = JsonSerializer.Serialize(value, configureOptions.JsonSerializerSettings);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absolute);

            await cache.SetStringAsync(key, json, options, cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                $"Set object in cache associative key {key}, absolute expiration {absolute.Seconds} seconds, and no sliding expiration");
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new DistributedCacheException();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation($"Removes object associated with key {key}");
            await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new DistributedCacheException();
        }
    }

    public async Task<bool> IsExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await cache.GetAsync(key, cancellationToken);
            if (value is null)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw new DistributedCacheException();
        }
    }
}
