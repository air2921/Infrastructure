namespace Infrastructure.Abstractions;

public interface ICacheClient
{
    public Task SetAsync<TObject>(string key, TObject value, TimeSpan absolute, TimeSpan sliding, CancellationToken cancellationToken = default) where TObject : class;
    public Task SetAsync<TObject>(string key, TObject value, TimeSpan absolute, CancellationToken cancellationToken = default) where TObject : class;
    public Task<TObject?> GetAsync<TObject>(string key, CancellationToken cancellationToken = default) where TObject : class;
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    public Task<bool> IsExists(string key, CancellationToken cancellationToken = default);
}
