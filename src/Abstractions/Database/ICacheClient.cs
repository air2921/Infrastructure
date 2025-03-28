using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions.Database;

/// <summary>
/// Interface for interacting with a distributed cache, providing methods for storing, retrieving, and managing cached data.
/// </summary>
public interface ICacheClient
{
    /// <summary>
    /// Asynchronously stores an object in the cache with the specified key, absolute expiration time, and sliding expiration time.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to store.</typeparam>
    /// <param name="key">The key to associate with the cached object.</param>
    /// <param name="value">The object to store in the cache.</param>
    /// <param name="absolute">The absolute expiration time for the cached object.</param>
    /// <param name="sliding">The sliding expiration time for the cached object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="DistributedCacheException">Thrown if there is an error while interacting with the cache, such as connectivity issues or invalid parameters.</exception>
    public Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, TimeSpan sliding, CancellationToken cancellationToken = default) where TValue : notnull;

    /// <summary>
    /// Asynchronously stores an object in the cache with the specified key and absolute expiration time.
    /// </summary>
    /// <typeparam name="TValue">The type of the object to store.</typeparam>
    /// <param name="key">The key to associate with the cached object.</param>
    /// <param name="value">The object to store in the cache.</param>
    /// <param name="absolute">The absolute expiration time for the cached object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="DistributedCacheException">Thrown if there is an error while interacting with the cache, such as connectivity issues or invalid parameters.</exception>
    public Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, CancellationToken cancellationToken = default) where TValue : notnull;

    /// <summary>
    /// Asynchronously retrieves an object from the cache associated with the specified key.
    /// </summary>
    /// <typeparam name="TResult">The type of the object to retrieve.</typeparam>
    /// <param name="key">The key associated with the cached object.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The cached object, or <c>null</c> if the key is not found.</returns>
    /// <exception cref="DistributedCacheException">Thrown if there is an error while retrieving the object from the cache.</exception>
    public Task<TResult?> GetAsync<TResult>(string key, CancellationToken cancellationToken = default) where TResult : class;

    /// <summary>
    /// Asynchronously removes an object from the cache associated with the specified key.
    /// </summary>
    /// <param name="key">The key associated with the cached object to remove.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="DistributedCacheException">Thrown if there is an error while removing the object from the cache.</exception>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously checks if an object exists in the cache associated with the specified key.
    /// </summary>
    /// <param name="key">The key associated with the cached object to check.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns><c>true</c> if the object exists in the cache; otherwise, <c>false</c>.</returns>
    /// <exception cref="DistributedCacheException">Thrown if there is an error while checking the cache.</exception>
    public Task<bool> IsExistsAsync(string key, CancellationToken cancellationToken = default);
}
