namespace Infrastructure.Abstractions;

public interface IS3Client
{
    public ValueTask<string> PreSignedUrlAsync(string bucket, string key, DateTime expires);
    public Task<Stream> DownloadAsync(string bucket, string key, CancellationToken cancellationToken = default);
    public Task UploadAsync(Stream stream, string bucket, string key, CancellationToken cancellationToken = default);
    public Task RemoveAsync(string bucket, string key, CancellationToken cancellationToken = default);
}
