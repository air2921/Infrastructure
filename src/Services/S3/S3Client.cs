using Amazon.S3;
using Amazon.S3.Model;
using Infrastructure.Abstractions.Database;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.S3;

/// <summary>
/// A client for interacting with Amazon S3, providing methods for generating pre-signed URLs, downloading, uploading, and deleting objects.
/// </summary>
/// <param name="s3Client">The Amazon S3 client used to perform operations.</param>
/// <param name="logger">A logger for tracking the operations performed by this client.</param>
/// <remarks>
/// This class encapsulates common S3 operations, such as generating pre-signed URLs, downloading, uploading, and deleting objects.
/// It handles exceptions internally and logs errors using the provided logger.
/// </remarks>
public class S3Client(IAmazonS3 s3Client, ILogger<S3Client> logger) : IS3Client
{
    private static readonly Lazy<S3ClientException> _s3SignedUrlError = new(() => new("An error occurred while attempting to create a signed reference to an object"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<S3ClientException> _s3DownloadingError = new(() => new("An error occurred while trying to unload an object from storage"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<S3ClientException> _s3UploadingError = new(() => new("An error occurred while trying to load the object into storage"), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<S3ClientException> _s3DeletingError = new(() => new("An error occurred while attempting to delete an object from storage"), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Generates a pre-signed URL for accessing an S3 object.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket containing the object.</param>
    /// <param name="key">The key (path) of the object in the S3 bucket.</param>
    /// <param name="expires">The expiration date and time for the pre-signed URL.</param>
    /// <returns>A pre-signed URL as a <see cref="ValueTask{string}"/>.</returns>
    /// <exception cref="S3ClientException">Thrown if an error occurs while generating the pre-signed URL.</exception>
    public ValueTask<string> PreSignedUrlAsync(string bucket, string key, DateTime expires)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Expires = expires,
                Verb = HttpVerb.GET
            };

            return new ValueTask<string>(s3Client.GetPreSignedURL(request));
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _s3SignedUrlError.Value;
        }
    }

    /// <summary>
    /// Downloads an object from an S3 bucket as a stream.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket containing the object.</param>
    /// <param name="key">The key (path) of the object in the S3 bucket.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{Stream}"/> representing the downloaded object stream.</returns>
    /// <exception cref="S3ClientException">Thrown if an error occurs while downloading the object.</exception>
    public async Task<Stream> DownloadAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            }, cancellationToken);

            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _s3DownloadingError.Value;
        }
    }

    /// <summary>
    /// Uploads a stream as an object to an S3 bucket.
    /// </summary>
    /// <param name="stream">The stream to upload.</param>
    /// <param name="bucket">The name of the S3 bucket to upload the object to.</param>
    /// <param name="key">The key (path) where the object will be stored in the S3 bucket.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="S3ClientException">Thrown if an error occurs while uploading the object.</exception>
    public async Task UploadAsync(Stream stream, string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = stream
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _s3UploadingError.Value;
        }
    }

    /// <summary>
    /// Deletes an object from an S3 bucket.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket containing the object.</param>
    /// <param name="key">The key (path) of the object in the S3 bucket.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="S3ClientException">Thrown if an error occurs while deleting the object.</exception>
    public async Task RemoveAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw _s3DeletingError.Value;
        }
    }
}
