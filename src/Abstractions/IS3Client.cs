using Infrastructure.Exceptions;

namespace Infrastructure.Abstractions;

/// <summary>
/// Defines methods for interacting with an S3-compatible storage service.
/// Provides functionality for generating pre-signed URLs, downloading, uploading, and removing objects in an S3 bucket.
/// </summary>
public interface IS3Client
{
    /// <summary>
    /// Generates a pre-signed URL for accessing an object in an S3 bucket.
    /// The URL is valid for a specific expiration time.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket.</param>
    /// <param name="key">The key (path) of the object in the bucket.</param>
    /// <param name="expires">The expiration date and time of the pre-signed URL.</param>
    /// <returns>A task that returns the pre-signed URL as a <see cref="string"/>.</returns>
    /// <exception cref="S3ClientException">Thrown when an error occurs during URL generation.</exception>
    public ValueTask<string> PreSignedUrlAsync(string bucket, string key, DateTime expires);

    /// <summary>
    /// Downloads an object from the specified S3 bucket.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket.</param>
    /// <param name="key">The key (path) of the object in the bucket.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the download operation (optional).</param>
    /// <returns>A task that returns a <see cref="Stream"/> representing the downloaded object data.</returns>
    /// <exception cref="S3ClientException">Thrown when an error occurs during the download operation.</exception>
    public Task<Stream> DownloadAsync(string bucket, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads an object to the specified S3 bucket.
    /// </summary>
    /// <param name="stream">The data stream to upload to the S3 bucket.</param>
    /// <param name="bucket">The name of the S3 bucket.</param>
    /// <param name="key">The key (path) for the uploaded object in the bucket.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the upload operation (optional).</param>
    /// <returns>A task representing the asynchronous upload operation.</returns>
    /// <exception cref="S3ClientException">Thrown when an error occurs during the upload operation.</exception>
    public Task UploadAsync(Stream stream, string bucket, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an object from the specified S3 bucket.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket.</param>
    /// <param name="key">The key (path) of the object to remove in the bucket.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the removal operation (optional).</param>
    /// <returns>A task representing the asynchronous removal operation.</returns>
    /// <exception cref="S3ClientException">Thrown when an error occurs during the removal operation.</exception>
    public Task RemoveAsync(string bucket, string key, CancellationToken cancellationToken = default);
}