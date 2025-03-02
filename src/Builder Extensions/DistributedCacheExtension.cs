using Infrastructure.Abstractions;
using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Infrastructure.Services.DistributedCache;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Builder_Extensions;

public static class DistributedCacheExtension
{
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
