using Amazon.S3;
using Amazon.S3.Model;
using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.S3;

public class S3Client(IAmazonS3 s3Client, ILogger<S3Client> logger) : IS3Client
{
    public ValueTask<string> PreSignedUrl(string bucket, string key, DateTime expires)
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
            throw new S3ClientException();
        }
    }

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
            throw new S3ClientException();
        }
    }

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
            throw new S3ClientException();
        }
    }

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
            throw new S3ClientException();
        }
    }
}
