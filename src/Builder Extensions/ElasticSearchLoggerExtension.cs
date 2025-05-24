using Infrastructure.Configuration;
using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Infrastructure.Builder_Extensions;

/// <summary>
/// Provides extension methods for adding ElasticSearch logging services to an <see cref="IInfrastructureBuilder"/>.
/// </summary>
public static class ElasticSearchLoggerExtension
{
    /// <summary>
    /// Adds Elasticsearch logging to the application infrastructure.
    /// </summary>
    /// <param name="builder">Infrastructure builder instance</param>
    /// <param name="configureOptions">Elasticsearch configuration action</param>
    /// <returns>The builder for chaining</returns>
    /// <remarks>
    /// Configures console and Elasticsearch logging with:
    /// - Correlation ID
    /// - Custom properties
    /// - Batched Elasticsearch writes (50 events/5sec)
    /// - Auto template registration
    /// Skips setup if disabled in options.
    /// </remarks>
    public static IInfrastructureBuilder AddElasticSearchLogger(this IInfrastructureBuilder builder, Action<ElasticSearchConfigureOptions> configureOptions)
    {
        var options = new ElasticSearchConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid ElasticSearch configuration. Please check connection string or sink options");

        var logggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(options.EventLevel)
            .Enrich.WithCorrelationId()
            .ReadFrom.Configuration(options.Configuration)
            .WriteTo.Console();

        foreach (var prop in options.Properties)
            logggerConfig.Enrich.WithProperty(prop.Key, prop.Value);

        logggerConfig.WriteTo.Elasticsearch(options.SinkOptions ?? new ElasticsearchSinkOptions(new Uri(options.Connection))
        {
            BatchPostingLimit = 50,
            Period = TimeSpan.FromSeconds(5),
            BufferFileSizeLimitBytes = 1024 * 1024,
            BufferRetainedInvalidPayloadsLimitBytes = 5000,
            AutoRegisterTemplate = true,
            OverwriteTemplate = true,
            IndexFormat = options.Index,
        });

        Log.Logger = logggerConfig.CreateLogger();

        builder.Services.AddLogging(log =>
        {
            log.AddSerilog(Log.Logger);
        });

        return builder;
    }
}
