using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding ElasticSearch logging services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class ElasticSearchLoggerExtension
{
    /// <summary>
    /// Adds ElasticSearch logging services to the <see cref="IInfrastructureBuilder"/>.
    /// </summary>
    /// <param name="builder">The infrastructure builder to which the ElasticSearch logging services will be added.</param>
    /// <param name="configureOptions">A delegate that configures the ElasticSearch options.</param>
    /// <returns>The updated <see cref="IInfrastructureBuilder"/> with the added ElasticSearch logging services.</returns>
    /// <exception cref="InfrastructureConfigurationException">
    /// Thrown when the ElasticSearch configuration is invalid, such as an incorrect connection string.
    /// </exception>
    public static IInfrastructureBuilder AddElasticSearchLogger(this IInfrastructureBuilder builder, Action<ElasticSearchConfigureOptions> configureOptions)
    {
        var options = new ElasticSearchConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        if (!options.IsValidConfigure())
            throw new InfrastructureConfigurationException("Invalid ElasticSearch configuration. Please check connection", nameof(options.Connection));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Assembly", Assembly.GetExecutingAssembly().GetName().Name)
            .Enrich.WithProperty(Immutable.EnvProperty, Environment.GetEnvironmentVariable(Immutable.AspNetCoreEnv)!)
            .WriteTo.Console()
            .ReadFrom.Configuration(options.Configuration)
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.Connection))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logs-{DateTime.UtcNow:yyyy}"
            })
            .CreateLogger();

        builder.Services.AddLogging(logger =>
        {
            logger.AddSerilog(Log.Logger);
        });

        return builder;
    }
}
