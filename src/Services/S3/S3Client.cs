using Amazon.S3;
using Amazon.S3.Model;
using Infrastructure.Abstractions.Database;
using Infrastructure.Data_Transfer_Object.S3;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.S3;

/// <summary>
/// A client for interacting with Amazon S3, providing methods for generating pre-signed URLs,
/// downloading, uploading, and deleting objects with detailed metadata.
/// </summary>
/// <param name="s3Client">The Amazon S3 client used to perform operations.</param>
/// <param name="logger">A logger for tracking the operations performed by this client.</param>
/// <remarks>
/// This class encapsulates common S3 operations with enhanced metadata tracking.
/// It handles exceptions internally and logs errors using the provided logger.
/// </remarks>
public class S3Client(IAmazonS3 s3Client, ILogger<S3Client> logger) : IS3Client
{
    /// <summary>
    /// Generates a pre-signed URL for accessing an S3 object with comprehensive metadata.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket containing the object.</param>
    /// <param name="key">The key (path) of the object in the S3 bucket.</param>
    /// <param name="expires">The expiration date and time for the pre-signed URL.</param>
    /// <returns>
    /// A <see cref="ValueTask{S3ObjectUrlDetails}"/> containing:
    /// - Generated URL
    /// - Object key
    /// - HTTP verb
    /// - Expiration time
    /// </returns>
    /// <exception cref="S3ClientException">Thrown if URL generation fails.</exception>
    public ValueTask<S3ObjectUrlDetails> PreSignedUrlAsync(string bucket, string key, DateTime expires)
    {
        try
        {
            var verb = HttpVerb.GET;

            var response = new S3ObjectUrlDetails
            {
                Expires = expires,
                Key = key,
                Verb = verb,
                Url = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = bucket,
                    Key = key,
                    Expires = expires,
                    Verb = verb
                })
            };

            return new ValueTask<S3ObjectUrlDetails>(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while attempting to create a signed reference to an object", [bucket, key]);
            throw new S3ClientException("An error occurred while attempting to create a signed reference to an object");
        }
    }

    /// <summary>
    /// Downloads an object from S3 with complete content metadata.
    /// </summary>
    /// <param name="bucket">The name of the S3 bucket containing the object.</param>
    /// <param name="key">The key (path) of the object in the S3 bucket.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Task{S3ObjectContentDetails}"/> containing:
    /// - Object content stream
    /// - Object key
    /// - Content size in bytes
    /// - Content type
    /// </returns>
    /// <exception cref="S3ClientException">Thrown if download fails.</exception>
    public async Task<S3ObjectContentDetails> DownloadAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            }, cancellationToken);

            return new S3ObjectContentDetails
            {
                Key = response.Key,
                Size = response.ContentLength,
                ContentType = response.Headers.ContentType,
                Content = response.ResponseStream
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while trying to unload an object from storage", [bucket, key]);
            throw new S3ClientException("An error occurred while trying to unload an object from storage");
        }
    }

    /// <summary>
    /// Uploads a stream to S3 storage.
    /// </summary>
    /// <param name="stream">The stream to upload.</param>
    /// <param name="bucket">The target S3 bucket name.</param>
    /// <param name="key">The destination key (path) in the bucket.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="S3ClientException">Thrown if upload fails.</exception>
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
            logger.LogError(ex, "An error occurred while trying to load the object into storage", [bucket, key]);
            throw new S3ClientException("An error occurred while trying to load the object into storage");
        }
    }

    /// <summary>
    /// Deletes an object from S3 storage.
    /// </summary>
    /// <param name="bucket">The S3 bucket containing the object.</param>
    /// <param name="key">The key (path) of the object to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <exception cref="S3ClientException">Thrown if deletion fails.</exception>
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
            logger.LogError(ex, "An error occurred while attempting to delete an object from storage", [bucket, key]);
            throw new S3ClientException("An error occurred while attempting to delete an object from storage");
        }
    }
}
