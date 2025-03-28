using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Infrastructure.Abstractions.Database;
using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.S3;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding Amazon S3 client services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class S3ClientExtension
{
    /// <summary>
    /// Adds Amazon S3 client services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the Amazon S3 client services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the S3 client options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added Amazon S3 client services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the S3 configuration is invalid, such as incorrect AccessKey, SecretKey, or Region.
    /// </exception>
    /// <remarks>
    /// This method registers the following services for Dependency Injection (DI):
    /// <list type="bullet">
    ///     <item><description><see cref="S3ConfigureOptions"/> - Singleton service for storing S3 configuration.</description></item>
    ///     <item><description><see cref="IAmazonS3"/> - Singleton service provided by AWS SDK for interacting with Amazon S3.</description></item>
    ///     <item><description><see cref="IS3Client"/> - Scoped service for interacting with Amazon S3 using a higher-level abstraction.</description></item>
    /// </list>
    /// Additionally, this method performs the following:
    /// <list type="bullet">
    ///     <item><description>Validates the S3 configuration (AccessKey, SecretKey, and Region).</description></item>
    ///     <item><description>Configures AWS credentials and region using <see cref="BasicAWSCredentials"/> and <see cref="RegionEndpoint"/>.</description></item>
    /// </list>
    /// </remarks>
    public static IInfrastructureBuilder AddS3Client(this IInfrastructureBuilder builder, Action<S3ConfigureOptions> configureOptions)
    {
        var options = new S3ConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid S3 configuration. Please check AccessKey, SecretKey, and Region");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = options.Endpoint,
            ForcePathStyle = true,
            UseHttp = options.Endpoint?.StartsWith("http://") == true,
        };

        var awsCredentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);

        builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(awsCredentials, s3Config));
        builder.Services.AddScoped<IS3Client, S3Client>();

        return builder;
    }
}
