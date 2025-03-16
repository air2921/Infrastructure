using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Infrastructure.Abstractions;
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
    public static IInfrastructureBuilder AddS3Client(this IInfrastructureBuilder builder, Action<S3ConfigureOptions> configureOptions)
    {
        var options = new S3ConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid S3 configuration. Please check AccessKey, SecretKey, and Region.", nameof(options));

        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            Region = RegionEndpoint.GetBySystemName(options.Region)
        };

        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddDefaultAWSOptions(awsOptions);

        builder.Services.AddScoped<IS3Client, S3Client>();

        return builder;
    }
}
