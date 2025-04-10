using Infrastructure.Abstractions.Database;
using Infrastructure.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.DistributedCache;

/// <summary>
/// A client for interacting with a distributed cache, providing methods for storing, retrieving, and managing cached data.
/// This class implements the <see cref="ICacheClient"/> interface to provide cache operations.
/// </summary>
/// <param name="cache">The distributed cache instance used for cache operations.</param>
/// <param name="logger">A logger for tracking cache operations and errors.</param>
/// <param name="configureOptions">Configuration options for the cache, including JSON serialization settings.</param>
/// <remarks>
/// This class provides methods to interact with a distributed cache, such as getting, setting, removing, and checking the existence of cached items.
/// It handles serialization and deserialization of objects using the provided JSON serializer settings.
/// </remarks>
public class CacheClient(IDistributedCache cache, ILogger<CacheClient> logger, CacheConfigureOptions configureOptions) : ICacheClient
{
    /// <summary>
    /// Retrieves an object from the cache associated with the specified key.
    /// </summary>
    /// <typeparam name="TResult">The type of the object to retrieve.</typeparam>
    /// <param name="key">The key associated with the cached object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The cached object, or <c>null</c> if the key is not found.</returns>
    /// <exception cref="DistributedCacheException">Thrown if an error occurs during the operation.</exception>
    public async Task<TResult?> GetAsync<TResult>(string key, CancellationToken cancellationToken = default) where TResult : class
    {
        try
        {
            logger.LogInformation("Received request to get object from cache", [key]);

            object? cacheObj = await cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (cacheObj is null)
                return null;

            if (typeof(TResult) == typeof(string))
                return cacheObj as TResult;

            return JsonSerializer.Deserialize<TResult>((string)cacheObj);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to retrieve data from the cache", [key, typeof(TResult)]);
            throw new DistributedCacheException("An error occurred while attempting to retrieve data from the cache");
        }
    }

    /// <summary>
    /// Stores an object in the cache with the specified key, absolute expiration, and sliding expiration.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to store.</typeparam>
    /// <param name="key">The key to associate with the cached object.</param>
    /// <param name="value">The object to store in the cache.</param>
    /// <param name="absolute">The absolute expiration time for the cached object.</param>
    /// <param name="sliding">The sliding expiration time for the cached object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="DistributedCacheException">Thrown if an error occurs during the operation.</exception>
    public async Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, TimeSpan sliding, CancellationToken cancellationToken = default) where TValue : notnull
    {
        try
        {
            var json = JsonSerializer.Serialize(value, configureOptions.JsonSerializerSettings);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absolute)
                .SetSlidingExpiration(sliding);

            await cache.SetStringAsync(key, json, options, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Received request to set object into cache", [key, typeof(TValue), absolute, sliding]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to cache data", [key, typeof(TValue), absolute.TotalSeconds, sliding.TotalSeconds]);
            throw new DistributedCacheException("An error occurred while attempting to cache data");
        }
    }

    /// <summary>
    /// Stores an object in the cache with the specified key and absolute expiration.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to store.</typeparam>
    /// <param name="key">The key to associate with the cached object.</param>
    /// <param name="value">The object to store in the cache.</param>
    /// <param name="absolute">The absolute expiration time for the cached object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="DistributedCacheException">Thrown if an error occurs during the operation.</exception>
    public async Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, CancellationToken cancellationToken = default) where TValue : notnull
    {
        try
        {
            var json = JsonSerializer.Serialize(value, configureOptions.JsonSerializerSettings);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(absolute);

            await cache.SetStringAsync(key, json, options, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Received request to set object into cache", [key, typeof(TValue), absolute]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to cache data", [key, typeof(TValue), absolute.TotalSeconds]);
            throw new DistributedCacheException("An error occurred while attempting to cache data");
        }
    }

    /// <summary>
    /// Removes an object from the cache associated with the specified key.
    /// </summary>
    /// <param name="key">The key associated with the cached object to remove.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="DistributedCacheException">Thrown if an error occurs during the operation.</exception>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to remove object from cache", [key]);
            await cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to remove data from the cache", [key]);
            throw new DistributedCacheException("An error occurred while attempting to remove data from the cache");
        }
    }

    /// <summary>
    /// Checks if an object exists in the cache associated with the specified key.
    /// </summary>
    /// <param name="key">The key associated with the cached object to check.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns><c>true</c> if the key exists in the cache; otherwise, <c>false</c>.</returns>
    /// <exception cref="DistributedCacheException">Thrown if an error occurs during the operation.</exception>
    public async Task<bool> IsExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Received request to check object existing in cache", [key]);

            var value = await cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (value is null)
                return false;

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to check if a key exists in the cache", [key]);
            throw new DistributedCacheException("An error occurred while attempting to check if a key exists in the cache");
        }
    }
}
