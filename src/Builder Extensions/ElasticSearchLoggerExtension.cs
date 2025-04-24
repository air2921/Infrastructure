using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;
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
    /// <remarks>
    /// This method configures logging services and registers the following components:
    /// <list type="bullet">
    ///     <item><description><see cref="ElasticSearchConfigureOptions"/> - Singleton service for storing ElasticSearch configuration.</description></item>
    ///     <item><description><see cref="Serilog.ILogger"/> - Global logger instance configured with Serilog, including ElasticSearch sink.</description></item>
    /// </list>
    /// Additionally, this method configures the following logging behaviors:
    /// <list type="bullet">
    ///     <item><description>Logs are enriched with context properties such as assembly name and environment.</description></item>
    ///     <item><description>Logs are written to both the console and ElasticSearch.</description></item>
    ///     <item><description>ElasticSearch index format is set to <c>logs-YYYY</c>, where <c>YYYY</c> is the current year.</description></item>
    /// </list>
    /// </remarks>
    public static IInfrastructureBuilder AddElasticSearchLogger(this IInfrastructureBuilder builder, Action<ElasticSearchConfigureOptions> configureOptions)
    {
        var options = new ElasticSearchConfigureOptions();
        configureOptions.Invoke(options);

        if (!options.IsEnable)
            return builder;

        options.EnsureSuccessValidation("Invalid ElasticSearch configuration. Please check connection");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(options.EventLevel)
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Assembly", Assembly.GetExecutingAssembly().GetName().Name)
            .Enrich.WithProperty(InfrastructureImmutable.ASPNETCore.EnvProperty, Environment.GetEnvironmentVariable(InfrastructureImmutable.ASPNETCore.AspNetCoreEnv)!)
            .ReadFrom.Configuration(options.Configuration)
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.Connection))
            {
                BatchPostingLimit = 50,
                Period = TimeSpan.FromSeconds(2),
                BufferFileSizeLimitBytes = 1024 * 1024,
                BufferRetainedInvalidPayloadsLimitBytes = 5000,
                AutoRegisterTemplate = true,
                OverwriteTemplate = true,
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true, formatStackTraceAsArray: true),
                IndexFormat = options.Index,
                TypeName = null
            })
            .CreateLogger();

        builder.Services.AddLogging(log =>
        {
            log.AddSerilog(Log.Logger);
        });

        return builder;
    }
}
