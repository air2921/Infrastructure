using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.DistributedCache;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding distributed cache services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class DistributedCacheExtension
{
    /// <summary>
    /// Adds distributed cache services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the distributed cache services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the cache options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added distributed cache services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the cache configuration is invalid, such as an incorrect Redis connection string.
    /// </exception>
    public static IInfrastructureBuilder AddDistributedCache(this IInfrastructureBuilder builder, Action<CacheConfigureOptions> configureOptions)
    {
        var options = new CacheConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid Redis configuration. Please check the connection string.", nameof(options.Connection));

        builder.Services.AddStackExchangeRedisCache(cache =>
        {
            cache.Configuration = options.Connection;
        });

        builder.Services.AddSingleton(options);
        builder.Services.AddScoped<ICacheClient, CacheClient>();

        return builder;
    }
}
