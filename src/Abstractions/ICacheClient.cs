namespace Infrastructure.Abstractions;

public interface ICacheClient
{
    public Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, TimeSpan sliding, CancellationToken cancellationToken = default) where TValue : notnull;
    public Task SetAsync<TValue>(string key, TValue value, TimeSpan absolute, CancellationToken cancellationToken = default) where TValue : notnull;
    public Task<TResult?> GetAsync<TResult>(string key, CancellationToken cancellationToken = default) where TResult : class;
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    public Task<bool> IsExists(string key, CancellationToken cancellationToken = default);
}
