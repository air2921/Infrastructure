﻿using Infrastructure.Configuration;
using Infrastructure.Exceptions.Global;
using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    /// <remarks>
    /// This method configures logging services and registers the following components:
    /// <list type="bullet">
    ///     <item><description><see cref="ElasticSearchConfigureOptions"/> - Singleton service for storing ElasticSearch configuration.</description></item>
    ///     <item><description><see cref="Serilog.ILogger"/> - Global logger instance configured with Serilog, including ElasticSearch sink.</description></item>
    ///     <item><description><see cref="ILogger{TCategoryName}"/> - Logging services integrated with ASP.NET Core's logging infrastructure.</description></item>
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
            logger.AddConsole();
        });

        return builder;
    }
}
